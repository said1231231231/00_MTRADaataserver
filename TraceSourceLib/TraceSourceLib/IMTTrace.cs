/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA Corporation.                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: Описание стратегии и контекста трассировки
 *				Используется шаблон проектирования Стратегия (поведение)			                                                                             
 *	Файл                     : X:\Projects\TraceSourceLib\TraceSourceLib\IMTTrace.cs
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

namespace TraceSourceLib
{
	/// <summary>
	/// Интерфейс «Стратегия» определяет функциональность, которая должна быть реализована
	/// конкретными классами стратегий. Другими словами, метод интерфейса определяет
	/// решение некой задачи, а его реализации в конкретных классах стратегий определяют,
	/// КАК, КАКИМ ПУТЁМ эта задача будет решена.
	/// </summary>
	public interface IMTTraceIStrategy
	{
		void CreateLog(string name);
		void CloseLog();
		void Stop();
		void Start();
		void FlushLog();

		/// <summary>
		/// вывод диагностического сообщения
		/// </summary>
		/// <param name="tet">уровень сообщения</param>
		/// <param name="idevent">id сообщения (|> номер строки)</param>
		/// <param name="msg">строка сообщения</param>
		void WtiteTraceMsg(TraceEventType tet, int idevent, string msg);

		/// <summary>
		/// вывод диагностического сообщения по исключению
		/// </summary>
		/// <param name="ex">исключение, из кот. извлекаются данные по покнтексту</param>
		void WtiteTraceMsg(Exception ex);

		/// <summary>
		/// вывод диагностического сообщения в виде фампа памяти
		/// </summary>
		/// <param name="tet">уровень сообщения</param>
		/// <param name="idevent">id сообщения (|> номер строки)</param>
		/// <param name="msg">строка сообщения</param>
		void WriteTraceDump(TraceEventType tet, int idevent, string dumpTitle, byte[] source);
	}

	/// <summary>
	/// Контекст, использующий стратегию для решения своей задачи.
	/// </summary>
	public class MTTraceContext : IMTTraceIStrategy
	{
			/// <summary>
			/// Ссылка на интерфейс IMTTraceIStrategy
			/// позволяет автоматически переключаться между конкретными реализациями
			/// (другими словами, это выбор конкретной стратегии).
			/// </summary>
		private IMTTraceIStrategy _strategy;

		/// <summary>
		/// Конструктор контекста.
		/// Инициализирует объект стратегией.
		/// </summary>
		/// <param name="strategy">
		/// Стратегия.
		/// </param>
		public MTTraceContext(IMTTraceIStrategy strategy)
		{
			_strategy = strategy;
		}

		/// <summary>
		/// Метод для установки стратегии.
		/// Служит для смены стратегии во время выполнения.
		/// В C# может быть реализован также как свойство записи.
		/// </summary>
		/// <param name="strategy">
		/// Новая стратегия.
		/// </param>
		public void SetStrategy(IMTTraceIStrategy strategy)
		{
			_strategy = strategy;
		}

		public void CreateLog(string name)
		{
			_strategy.CreateLog(name);
		}

		public void CloseLog()
		{
			_strategy.CloseLog();
		}

		public void Stop()
		{
			_strategy.Stop();
		}

		public void Start()
		{
			_strategy.Start();
		}

		public void FlushLog()
		{
			_strategy.FlushLog();
		}

		public void WtiteTraceMsg(TraceEventType tet, int idevent, string msg)
		{
			_strategy.WtiteTraceMsg(tet, idevent, msg);
		}

		public void WtiteTraceMsg(Exception ex)
		{
			_strategy.WtiteTraceMsg(ex);
		}

		public void WriteTraceDump(TraceEventType tet, int idevent, string dumpTitle, byte[] source)
		{
			_strategy.WriteTraceDump(tet, idevent, dumpTitle, source);
		}
	}
}
