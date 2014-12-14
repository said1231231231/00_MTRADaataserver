/*##############################################################################
 *    Copyright (C) 2006-2009 Mehanotronika Corporation.                       *
 *    All rights reserved.                                                     *
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*
 *                                                                             *
 *	Описание:  Описание общих классов уровня извлечения данных из сети                        *
 *								                                               *
 *	Файл                     : NetNetManager.cs                                   *
 *	Тип конечного файла      : Библиотека классов                                   *
 *	версия ПО для разработки : С#, Framework 3.5                               *
 *	Разработчик              : Юров В.И.                                       *
 *	Дата начала разработки   : 21.12.2008                                      *
 *	Дата (v1.0)              :                                                 *
 *******************************************************************************
 * Изменения:
 * 1. Дата(Автор): ...cодержание...
 *#############################################################################*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.IO.Compression;
using System.Xml.XPath;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

namespace uvs_MOA.MOA_ECU_SOURCE
{
   /// <summary>
   /// class ChatUdpClient
   /// класс, представляющий UdpClient, рассчитанный на broadcast
   /// </summary>
   public class ChatUdpClient : IDisposable
   {
      private UdpClient _client = new UdpClient ( );
      private IPEndPoint _remoteEndPoint;

      /// <summary>
      /// Данный консруктор служит для инициализации объекта UdpClient, работающего через broadcasting.
      /// </summary>
      /// <param name="port"></param>
      public ChatUdpClient ( int port )
      {
         // Процесс переконвертации адреса прокси в broadcast адрес.
         // Например, 192.168.100.1 --> 192.168.100.255
         Uri proxyAddress = WebProxy.GetDefaultProxy ( ).Address;

          // для использования одного порта несколькими приложениями
         _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

         // Если компьютер не подключен к локальной сети, то используем адрес "127.0.0.1"
         byte[] address = IPAddress.Parse ( ( proxyAddress != null ) ? proxyAddress.Host : "127.0.0.1" ).GetAddressBytes ( );

         // Если IPv4 формат
         if ( address.Length == 4 )
         {
            address[ 3 ] = 255;
            byte[] buffer = new byte[ 8 ];
            address.CopyTo ( buffer , 0 );
            address = buffer;
         }
         else if ( address.Length == 8 )
         {
            address[ 7 ] = 255;
         }

         // К сожалению, конструктор IPAddress ожидает массив байт, олицетворяющие IPv6
         InitRemoteEndPoint ( new IPAddress ( BitConverter.ToInt64 ( address , 0 ) ) , port );
      }


      /// <summary>
      /// Данный консруктор служит для иниицализации объекта UdpClient, работающего через multicasting.
      /// </summary>
      /// <param name="address"></param>
      /// <param name="port"></param>
      public ChatUdpClient ( IPAddress address , int port )
      {
         InitRemoteEndPoint ( address , port );
      }

      public void InitRemoteEndPoint ( IPAddress address , int port )
      {
         _remoteEndPoint = new IPEndPoint ( address , port );
      }

      public void SendMessage ( Byte[ ] btr )
      {
         try
         {
            _client.Send ( btr , btr.Length , _remoteEndPoint );
         }
         catch
         {
            System.Diagnostics.Trace.TraceInformation ( "\n" + DateTime.Now.ToString ( ) + " : NetNetManager : SendMessage : " );
            return;
         }
      }

      #region IDisposable Members

      public void Dispose ( )
      {
         _client.Close ( );
      }

      #endregion
   }

   /// <summary>
   /// class ChatUdpListener 
   /// класс, представляющий UdpClient, рассчитанный на multycast
   /// </summary>
   public class ChatUdpListener : InteractiveServer
   {
      private class ReceiveState
      {
         private AutoResetEvent _evt;

         public ReceiveState ( AutoResetEvent evt )
         {
            _evt = evt;
         }

         public byte[] Buffer = new byte[ BufferSize ];
         public const int BufferSize = 1468 + 10;   //1024

         public bool SetEvent ( )
         {
            return _evt.Set ( );
         }
      }

      private UdpClient _udpClient;

      private Socket _listener;

      public ChatUdpListener ( int port )
         : this ( null , port )
      {
      }

      public ChatUdpListener ( IPAddress multicastAddress , int port )
      {
         _udpClient = new UdpClient();//port
          // задание опций сокета для группового использования порта UDP
         _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
         _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));//IPAddress.Loopback //IPAddress.Parse("192.168.240.16")


         if ( multicastAddress != null )
         {
            _multicastAddress = multicastAddress;

            // Записываемся на рассылку.
            _udpClient.JoinMulticastGroup ( _multicastAddress );
         }
         Type ty = _udpClient.GetType ( );
         PropertyInfo tt = _udpClient.GetType ( ).GetProperty ( "Client" );
         _listener = tt.GetValue ( _udpClient , null ) as Socket;

         base.Start ( );
      }

      private IPAddress _multicastAddress;

      public IPAddress MulticastAddress
      {
         get { return _multicastAddress; }
      }

      private void OnBeginReceiveFrom ( IAsyncResult ar )
      {
          try
          {
              ReceiveState state = ar.AsyncState as ReceiveState;
              EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
              _listener.EndReceiveFrom(ar, ref endPoint);

              byte[] dataNet = state.Buffer;  //
              base.RaiseEvent(new NewMessage(endPoint, dataNet));
              state.SetEvent();
          }
          catch (Exception ex)
          {
              TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
              TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 194, string.Format("{0} : {1} : {2} : DataServer прекращает работу из-за нарушения сетевого обмена. Требуется перезапуск DataServer.", DateTime.Now.ToString(), @"X:\Projects\00_DataServer\ProviderCustomerExchangeLib\NSPublic.cs", "OnBeginReceiveFrom()"));
              
              Process.GetCurrentProcess().Kill();
          }
      }

      protected override void OnProcessRequest ( )
      {
         EndPoint endPoint = new IPEndPoint ( IPAddress.Any , 0 );
         AutoResetEvent evt = new AutoResetEvent ( false );
         ReceiveState state = new ReceiveState ( evt );

         _listener.BeginReceiveFrom ( state.Buffer , 0 , ReceiveState.BufferSize , SocketFlags.None , ref endPoint , new AsyncCallback ( OnBeginReceiveFrom ) , state );
         evt.WaitOne ( );
      }

      protected override void Dispose ( bool disposing )
      {
         // Если сценарий UDP-коммуникации как multicast, то оптисываемся от рассылки.
         if ( _multicastAddress != null )
            _udpClient.DropMulticastGroup ( _multicastAddress );
         if (_udpClient != null)
            _udpClient.Close ( );

         base.Dispose ( );
      }
   }

   /****************************************************************************
    *	class NewMessage
    * класс, представляющий EventArgs
    ****************************************************************************/
   public class NewMessage : EventArgs
   {
      public NewMessage ( EndPoint endPoint , byte[ ] message )
      {
         _endPoint = endPoint;
         _message = message;
      }

      private EndPoint _endPoint;

      public EndPoint EndPoint
      {
         get { return _endPoint; }
      }

      private byte[] _message;

      public byte[ ] Message
      {
         get { return _message; }
      }
   }

   /// <summary>
   /// Summary description for InteractiveServer.
   /// </summary>
   public abstract class InteractiveServer : BaseServer
   {
	   public event EventHandler NewMessage
	   {
		   add { _newMessage += value; }
		   remove { _newMessage -= value; }
	   }
	   private event EventHandler _newMessage;

	   public void RaiseEvent(EventArgs e)
	   {
		   if (_newMessage != null)
			   _newMessage(this, e);
	   }
   }

   public abstract class BaseServer : IDisposable
   {
	   private const int _stoppedState = 0;
	   private const int _startedState = 1;

	   private Thread _listenThread;
	   private int _serverState = _stoppedState;

	   public void Start()
	   {
		   if (_serverState == _stoppedState)
		   {
			   _serverState = _startedState;
			   _listenThread = new Thread(new ThreadStart(OnListen));
			   _listenThread.IsBackground = true;
			   _listenThread.Start();
		   }
	   }

	   public void Stop()
	   {
		   Dispose();
	   }

	   private void OnListen()
	   {
		   //while (Interlocked.CompareExchange(ref _serverState, _stoppedState, _stoppedState) != _stoppedState)
		   while (_serverState != _stoppedState)
		   {
			   OnProcessRequest();
		   }
	   }

	   protected abstract void OnProcessRequest();

	   protected virtual void Dispose(bool disposing)
	   {
		   //if (Interlocked.CompareExchange(ref _serverState, _stoppedState, _startedState) == _stoppedState)
		   //	Interlocked.Decrement(ref _hostState);

		   _serverState = _stoppedState;
	   }

	   private bool _disposed;

	   #region IDisposable Members

	   public void Dispose()
	   {
		   if (_disposed == false)
		   {
			   _disposed = true;
			   Dispose(true);
			   GC.SuppressFinalize(this);
		   }
	   }

	   #endregion

	   ~BaseServer()
	   {
		   if (_disposed == false)
		   {
			   _disposed = true;
			   Dispose(false);
		   }
	   }
   }

}
