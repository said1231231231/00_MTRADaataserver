/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: Классы для создания очереди вх пакетов
 *                                                                             
 *	Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\MOA_ECU_SOURCE\PacketHandler.cs                  
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 17.10.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace uvs_MOA.MOA_ECU_SOURCE
{
   public delegate void PacketAppearance(Queue<byte[]> pq);

   public class PacketQueque
   {
      public event PacketAppearance packetAppearance;

      #region Свойства
      /// <summary>
      /// число элементов в очереди
      /// </summary>
      public int Count
      {
         get
         {
            return NetPackQ.Count;
         }
      }

      #endregion
      private Queue<byte[]> NetPackQ;
      
      /// <summary>
      /// Конструктор
      /// </summary>
      public PacketQueque()
      {
         // создаем очередь для пакетов последовательностей
         NetPackQ = new Queue<byte[]>();
      }
      /// <summary>
      /// добавить пакет в очередь
      /// </summary>
      /// <param name="bytearr"></param>
      public void Add(byte [] bytearr) 
      {
         NetPackQ.Enqueue(bytearr);
      }
      /// <summary>
      /// извлечь элемент из очереди
      /// </summary>
      /// <returns></returns>
      public byte[] Get( )
      {
         return (byte[])NetPackQ.Dequeue();
      }
      /// <summary>
      /// очистить очередь
      /// </summary>
      public void Clear() 
      {
         NetPackQ.Clear();
      }
      /// <summary>
      /// запустить процесс обработки пакетов в очереди
      /// </summary>
      public void ParsePackInQueque()
      {
         OnPacketAppearance();
      }
      /// <summary>
      /// событие - очередь не пуста
      /// </summary>
      private void OnPacketAppearance() 
      {
         if (packetAppearance != null)
            packetAppearance(NetPackQ);
      }
   }
}
