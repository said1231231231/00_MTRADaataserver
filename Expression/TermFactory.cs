/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: фабрика термов
 *                                                                             
 *	Файл                     : X:\Projects\38_DS4BlockingPrg\ExpressionSample\TermFactory.cs                                    
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 19.09.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Expression
{
	public class TermFactory
	{
		public ITerm CreateTerm(Match tok, string typeterm)
		{
			ITerm term = null;
			try
			{
				switch (typeterm.ToLower())
				{
					case "analog":
						term = (ITerm)new SingleTerm();
						break;
					case "discret":
						term = (ITerm)new BooleanTerm();
						break;
                    case "enum":
                        term = (ITerm)new EnumTerm();   //SingleTerm();
                        break;
                    case "analogconst":
                        term = (ITerm)new SingleTerm();
                        term.TermValue = Single.Parse(tok.Value);
                        break;
                    case "discretconst":
                        term = (ITerm)new BooleanTerm();
                        term.TermValue = Boolean.Parse(tok.Value);
                        break;

                    case "int16":
                        term = (ITerm)new SingleTerm();
                        break;
                    case "float":
                        term = (ITerm)new SingleTerm();
                        break;
                    case "boolean":
                        term = (ITerm)new BooleanTerm();
                        break;
                    default:
						throw new Exception(string.Format("Тип члена выражения {0} не поддерживается", typeterm));
				}
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
			}
			return term;
		}
	}
}
