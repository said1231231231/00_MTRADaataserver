/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA Corporation.                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: Конкретизация реализации стратегии трассировки
 *				по шаблону Стратегия (см. TraceSourceLib\IMTTrace.cs)
 *				трассировка средствами .net				                                                                             
 *	Файл                     : X:\Projects\TraceSourceLib\TraceSourceLib\DotNetTraceLog.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 11.02.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
 * Изменения:
 * 1. Дата(Автор): ...cодержание...
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualBasic.Logging;
using System.IO;

namespace TraceSourceLib
{
	public class DotNetTraceLog : IMTTraceIStrategy
	{
		private TraceSource tracesource;
		private bool isTraceStop = false;
		//static FileLogTraceListener fileLogTraceListener; // слушатель в файл
		/// <summary>
		/// temp-строка
		/// </summary>
		StringBuilder strmsg = new StringBuilder();

		/// <summary>
		/// Создание лога трассировки
		/// </summary>
		/// <param name="name"></param>
		public void CreateLog(string name)
		{
			// создаем Trace
			tracesource = new TraceSource(name);
			isTraceStop = false;
		}

		/// <summary>
		/// закрытие лога трассировки
		/// </summary>
		public void CloseLog()
		{
			tracesource.Flush();
			tracesource.Close();
		}

		/// <summary>
		/// старт трассировки
		/// </summary>
		public void Start()
		{
			isTraceStop = false;
		}

		/// <summary>
		/// остановка трассировки
		/// </summary>
		public void Stop()
		{
			isTraceStop = true;
		}

		/// <summary>
		/// Освобождение буфера трассировки
		/// </summary>
		public void FlushLog()
		{
			tracesource.Flush();			
		}

		/// <summary>
		/// вывод диагностического сообщения
		/// </summary>
		/// <param name="tet">уровень сообщения</param>
		/// <param name="idevent">id сообщения (|> номер строки)</param>
		/// <param name="msg">строка сообщения</param>
		public void WtiteTraceMsg(TraceEventType tet, int idevent, string msg)
		{
			if (tracesource == null || isTraceStop)
				return;

			tracesource.TraceEvent(tet, idevent, msg);
		}

		/// <summary>
		/// вывод диагностического сообщения по исключению
		/// </summary>
		/// <param name="ex">исключение, из кот. извлекаются данные по покнтексту</param>
		public void WtiteTraceMsg(Exception ex)
		{
			if (tracesource == null || isTraceStop)
				return;

			strmsg.Clear();

			strmsg.Append(DateTime.Now.ToString() + " : " + ex.Source + " : " + ex.TargetSite + "\n" + ex.ToString() + "\n" + ex.StackTrace);

			tracesource.TraceEvent(TraceEventType.Critical, 0, strmsg.ToString());
			tracesource.Flush();
		}

		/// <summary>
		/// вывод диагностического сообщения в виде фампа памяти
		/// </summary>
		/// <param name="tet">уровень сообщения</param>
		/// <param name="idevent">id сообщения (|> номер строки)</param>
		/// <param name="msg">строка сообщения</param>
		public void WriteTraceDump(TraceEventType tet, int idevent, string dumpTitle, byte[] source)
		{
			strmsg.Clear();

			if (tracesource == null || isTraceStop)
				return;

			tracesource.TraceEvent(tet, idevent, dumpTitle);	// заголовок дампа

			if (source.Length == 0)
				tracesource.TraceEvent(tet, idevent, "Содержимое пакета : пустой пакет.");
			else
				tracesource.TraceData(tet, idevent, source);
		}
	}
}
