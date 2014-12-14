/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: класс Parser,
 *	            для разбора формулы
 *                                                                             
 *	Файл                     : X:\Projects\38_DS4BlockingPrg\ExpressionSample\Parser.cs
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
using System.Globalization;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using CommonClassesLib.CommonClasses;
//using InterfaceLibrary;

namespace Expression
{
	public class Parser
	{
		/// <summary>
		/// регулярное выражение числа с ПТ
		/// (разделитель - точка)
		/// </summary>
		private const string REGEXP_NUMBER = "[0-9]+(.[0-9]+)?";
		/// <summary>
		/// регулярное выражение логическая константа
		/// </summary>
		private const string REGEXP_LOGICALCONST = "false|true|False|True";
        /// <summary>
        /// регулярное выражение - задание функции  в формате:
        /// FUNC[имя_функции(аргументы)]
        /// </summary>
        private const string REGEXP_FUNC = @"FUNC\[\w+\([a-zA-Z_0-9\.,]+\)\]";
		/// <summary>
		/// текущий терм формулы
		/// </summary>
		private Match token;
		/// <summary>
		/// тип результата выражения
		/// </summary>
		private string typeExpr = string.Empty;
		/// <summary>
		/// список соответствия идентиф и типа тега
		/// </summary>
        private SortedList<string, string> slTagsType = new SortedList<string, string>();
		/// <summary>
		/// список id тегов и их значений
		/// </summary>
        private /*public*/ Dictionary<string, Tuple<object, ProjectCommonData.VarQuality>> slexp_terms_values = new Dictionary<string, Tuple<object, ProjectCommonData.VarQuality>>();
		private TermFactory termFactory = new TermFactory();
		/// <summary>
		/// уникальный номер dataserver для замены в id тега
		/// </summary>
		private string unidstag = string.Empty;
        /// <summary>
        /// уникальный номер устройства для замены в id тега
        /// </summary>
        private string unidevguid = string.Empty;
		/// <summary>
		/// конструктор
		/// </summary>
		/// <param name="expr"></param>
		/// <param name="typeexpr"></param>
		/// <param name="sltagstype"></param>
        public Parser(string expr, string typeexpr, SortedList<string, string> sltagstype, string UniDS_GUID, string UniDev_GUID)
		{
			unidstag = UniDS_GUID.Trim();
            unidevguid = UniDev_GUID.Trim();

			//slexp_terms_values = slexp_terms_val;

			// заменяем номер локального ds (0) на реальный
			string rez = Regex.Replace(expr, @"[\d]+\.[\d]+\.[\d]+|", new MatchEvaluator(ComputeReplacment));
            
			token = Regex.Match(rez,
				@"[\d]+\.[\d]+\.[\d]+|" +	// идентификатор тега в формате ds.dev.tagguid			
				REGEXP_NUMBER + "|" +		// число с ПТ - разделитель '.' согласно регулярному выражению REGEXP_NUMBER
				"\\+|\\-|\\*|/|" +			// арифметические операторы
				"\\(|\\)" + "|" +			// скобки
				"&"  + "|" +				// логические операции And Or Xor Not
				"\\|" + "|" +
				"\\^" + "|" +
				"\\!=" + "|" +
				REGEXP_LOGICALCONST + "|" +	// логические константы
                REGEXP_FUNC + "|" +	// вызов функции
				"\\<=" + "|" + "\\>=" + "|" +			// операции отношения
				"==" + "|" + "\\!" + "|" +
				"\\<" + "|" + "\\>"
				);
			
			typeExpr = typeexpr;
			slTagsType = sltagstype;			
		}

		/// <summary>
		/// делегат замены
		/// </summary>
		/// <param name="matchResult"></param>
		/// <returns></returns>
		private string ComputeReplacment(Match matchResult)
		{
            // если номер устройства = 0, то необходимо его заменить на DevGUID текущего устройства
            string rez = matchResult.Value;

            try
            {
                if (rez.Split(new char[] { '.' }).Length == 3)
                    if (rez.Split(new char[] { '.' })[1].Trim() == "0")
                        rez = Regex.Replace(rez, @"^[\d]+\.0\.", string.Format("{0}.{1}.", unidstag, unidevguid));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return Regex.Replace(rez, @"^[\d]+\.", string.Format("{0}.", unidstag));
		}

		/// <summary>
		/// разбор выражения
		/// </summary>
		/// <returns></returns>
		public Expression Parse()
		{
			try
			{
				checkToken();
				Expression result = null;
				OpCode op = OpCodes.Add;

                while (token.Success && isFUNC())
                {
                    op = OpCodes.Call;
                    //token = token.NextMatch();
                    result = new FUNCExpression(token.Value);
                    //slexp_terms_values = (result as FUNCExpression).slIdVal;
                    token = token.NextMatch();
                    return result;
                }

				if (isAddOp())
				{
					op = token.Value.Equals("-") ? OpCodes.Sub : OpCodes.Add;
					token = token.NextMatch();
				}
				else if (isLogicalAdd())
				{
					op = token.Value.Equals("|") ? OpCodes.Or : OpCodes.Xor;
					token = token.NextMatch();
				}
				else if (isLogicalNeg())
				{
					op = OpCodes.Neg;
					token = token.NextMatch();
				}

				result = parseTerm();

				if (op.Equals(OpCodes.Sub))
					result = new UnaryExpression(result);

				if (op.Equals(OpCodes.Neg))
					result = new UnaryExpression(result);

				while (token.Success && isAddOp())
				{
					op = token.Value.Equals("-") ? OpCodes.Sub : OpCodes.Add;
					token = token.NextMatch();
					result = new BinaryExpression(result, parseTerm(), op);
				}

				while (token.Success && isLogicalAdd())
				{
					op = token.Value.Equals("|") ? OpCodes.Or : OpCodes.Xor;
					token = token.NextMatch();
					result = new BinaryExpression(result, parseTerm(), op);
				}
                
				return result;
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				throw ex;
			}
		}

		/// <summary>
		/// разбор терма
		/// </summary>
		/// <returns></returns>
		private Expression parseTerm()
		{
			try
			{
				checkToken();
				Expression result = parseFactor();

				while (token.Success && isMulOp())
				{
					OpCode op = token.Value.Equals("*") ?
					   OpCodes.Mul : OpCodes.Div;
					token = token.NextMatch();
                    Expression expp = new BinaryExpression(result, parseFactor(), op);

                    expp.slexp_terms_values = slexp_terms_values;
                    result = expp;
				}
                /*
                 * когда разбирался с ПР по поводу
                 * того что неправильно формировался результат и качество
                 * в большой формуле для мнемосхемы, то не хватало
                 * функции ниже для операции сложения
                 * впредь в подобных случаях можно смотреть здесь ...
                 */
                while (token.Success && isAddOp())
                {
                    OpCode op = token.Value.Equals("+") ?
                       OpCodes.Add : OpCodes.Sub;
                    token = token.NextMatch();
                    Expression expp = new BinaryExpression(result, parseFactor(), op);

                    expp.slexp_terms_values = slexp_terms_values;
                    result = expp;
                }

				while (token.Success && isLogicalAnd())
				{
					OpCode op = OpCodes.And;
					token = token.NextMatch();

                    Expression expp = new BinaryExpression(result, parseFactor(), op);
                    expp.slexp_terms_values = slexp_terms_values;
                    result = expp;
				}

                while (token.Success && isLogicalOr())
                {
                    OpCode op = OpCodes.Or;
                    token = token.NextMatch();

                    Expression expp = new BinaryExpression(result, parseFactor(), op);
                    expp.slexp_terms_values = slexp_terms_values;
                    result = expp;
                }
				while (token.Success && isCmpOp())
				{
					OpCode op = OpCodes.Nop;

					switch (token.Value)
					{ 
						case "<":
							op = OpCodes.Prefix1;
							break;
						case ">":
							op = OpCodes.Prefix2;
							break;
						case "==":
							op = OpCodes.Prefix3;
							break;
						case "!=":
							op = OpCodes.Prefix4;
							break;
						case ">=":
							op = OpCodes.Prefix5;
							break;
						case "<=":
							op = OpCodes.Prefix6;
							break;
						default:
							throw new Exception("Parser.cs : Parser.parseTerm() : (184) : Несуществующая операция логич сравнения");
					}
					
					token = token.NextMatch();

                    Expression expp = new BinaryExpression(result, parseFactor(), op);

                    expp.slexp_terms_values = slexp_terms_values;
                    result = expp;
				}

				return result;
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				throw ex;
			}
		}

		/// <summary>
		/// разбор множителя
		/// </summary>
		/// <returns></returns>
		private Expression parseFactor()
		{
            bool bval;
            Single sval;
			try
			{
				checkToken();
				Expression result = null;

				if (isIdentifier())
				{
                    if (!slTagsType.ContainsKey(token.Value))
                        throw new Exception("Parser.cs : Parser.parseFactor() : (240) : Несуществующий идентификатор");
                    else
                    {
                        if (!slexp_terms_values.ContainsKey(token.Value))
                            slexp_terms_values.Add(token.Value, new Tuple<object, ProjectCommonData.VarQuality>(string.Empty, ProjectCommonData.VarQuality.vqUndefined));
                    }

                    Expression expp = new VariableExpression(token, slTagsType[token.Value]);
                    expp.slexp_terms_values = slexp_terms_values;
                    result = expp;
				}
				else if (isNumber())
				{
                        // проверим десятичный разделитель
                        CultureInfo cinfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                        if (token.Value.Contains("."))
                            cinfo.NumberFormat.NumberDecimalSeparator = ".";
                        else if (token.Value.Contains(","))
                            cinfo.NumberFormat.NumberDecimalSeparator = ",";

                        Thread.CurrentThread.CurrentCulture = cinfo;

						if (Single.TryParse(token.Value, out sval))
							result = new ConstExpression(token, "AnalogConst");
						else
							throw new Exception("Parser.cs : Parser.parseFactor() : (207) : Некорректная числовая константа");
				}
				else if (isLogicalConst())
				{
					if (Boolean.TryParse(token.Value.ToLower(), out bval))
						result = new ConstExpression(token, "DiscretConst");
					else
						throw new Exception("Parser.cs : Parser.parseFactor() : (215) : Некорректная логическая константа");
				}
				else if (token.Value.Equals("("))
				{
					token = token.NextMatch();
					result = Parse();

					if (!token.Value.Equals(")"))
						throw new Exception(string.Format("Parser.cs : Parser.parseFactor() : (159) : требуется )"));
				}
                else if (isFUNC())
                {
                    result = new FUNCExpression(string.Empty);
                }
                else
					throw new Exception(string.Format("Parser.cs : Parser.parseFactor() : (162) : требуется ("));

				token = token.NextMatch();

				return result;
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				throw ex;
			}
		}

		/// <summary>
		/// проверка знака
		/// </summary>
		private void checkToken()
		{
			try
			{
				if (!token.Success)
					throw new Exception("Parser.cs : Parser.checkToken() : (183) : синтаксическая ошибка");
			}
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
				throw ex;
			}
		}

		/// <summary>
		/// проверка очередного символа на 
		/// соответсвие типу - идентификатор
		/// </summary>
		/// <returns></returns>
		private bool isIdentifier()
		{
            //if (Regex.IsMatch(token.Value, @"[\d]+\.[\d]+\.[\d]+"))
            //    if (!slexp_terms_values.ContainsKey(token.Value))	
            //        slexp_terms_values.Add(token.Value, new Tuple<string, VarQuality>(string.Empty, VarQuality.vqUndefined) );

			return Regex.IsMatch(token.Value, @"[\d]+\.[\d]+\.[\d]+");
		}

		/// <summary>
		/// очередной терм - число (константа)
		/// </summary>
		/// <returns></returns>
		private bool isNumber()
		{
			return Regex.IsMatch(token.Value, REGEXP_NUMBER);
		}

		/// <summary>
		/// очередной терм - логическая константа 
		/// false|true
		/// </summary>
		/// <returns></returns>
		private bool isLogicalConst()
		{
			return Regex.IsMatch(token.Value, REGEXP_LOGICALCONST);
		}

		/// <summary>
		/// терм - операция сложения/вычитания
		/// </summary>
		/// <returns></returns>
		private bool isAddOp()
		{
			return Regex.IsMatch(token.Value, "\\+|\\-");
		}

		/// <summary>
		/// терм - операция умножения/деления
		/// </summary>
		/// <returns></returns>
		private bool isMulOp()
		{
			return Regex.IsMatch(token.Value, "\\*|/");
		}

		/// <summary>
		/// терм - операция сравнения >|<
		/// </summary>
		/// <returns></returns>
		private bool isCmpOp()
		{
			return Regex.IsMatch(token.Value, "\\<|>|==|\\!=|\\<=|\\>=");
		}

		/// <summary>
		/// терм - операция логич сложения
		/// </summary>
		/// <returns></returns>
		private bool isLogicalAdd()
		{
			return Regex.IsMatch(token.Value, "\\||\\^");
		}
		/// <summary>
		/// терм - операция логич отрицания
		/// </summary>
		/// <returns></returns>
		private bool isLogicalNeg()
		{
			return Regex.IsMatch(token.Value, "\\!");
		}

		/// <summary>
		/// терм - операция логич умножения
		/// </summary>
		/// <returns></returns>
		private bool isLogicalAnd()
		{
			return Regex.IsMatch(token.Value, "&");
		}
        
		/// <summary>
		/// терм - операция логич умножения
		/// </summary>
		/// <returns></returns>
        private bool isLogicalOr()
		{
			return Regex.IsMatch(token.Value, "\\|");
		}
        /// <summary>
        /// очередной терм - вызов функции 
        /// FUNC[имя_функции(аргументы)]
        /// </summary>
        /// <returns></returns>
        private bool isFUNC()
        {
            return Regex.IsMatch(token.Value, REGEXP_FUNC);
        }
	}
}
