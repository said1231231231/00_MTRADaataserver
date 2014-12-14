/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: класс Expression,
 *	            для построения дерева выражений
 *                                                                             
 *	Файл                     : X:\Projects\38_DS4BlockingPrg\ExpressionSample\Expression.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using CommonClassesLib.CommonClasses;

namespace Expression
{
	public abstract class Expression
	{
        /// <summary>
        /// список соответствия id тега и его значения + качества
        /// </summary>
        public Dictionary<string, Tuple<object, ProjectCommonData.VarQuality>> slexp_terms_values = new Dictionary<string, Tuple<object, ProjectCommonData.VarQuality>>();
        
        /// <summary>
		/// фабрика термов
		/// </summary>
		public	TermFactory termFact = new TermFactory();

		/// <summary>
		/// вычислить дерево выражения
		/// </summary>
		/// <param name="slexp_terms_values"></param>
		/// <returns></returns>
        public abstract ITerm Evaluate();//Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values
	}

	/// <summary>
	/// класс унарное выражение:
    /// ! - логич отрицание
    /// - - -5
	/// </summary>
	class UnaryExpression : Expression
	{
		private Expression a;

		ITerm rez;

		public UnaryExpression(Expression a)
		{
			this.a = a; 
		}

        public override ITerm Evaluate()//Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values
		{
			try
			{
                foreach (KeyValuePair<string, Tuple<object, ProjectCommonData.VarQuality>> kvp in slexp_terms_values)
                {
                    if (a.slexp_terms_values.ContainsKey(kvp.Key))
                        a.slexp_terms_values[kvp.Key] = new Tuple<object, ProjectCommonData.VarQuality>(kvp.Value.Item1, kvp.Value.Item2);
                }

                rez = a.Evaluate();//slexp_terms_values
                if (rez.TermValue is Single)		
                    rez.TermValue = -(Single)rez.TermValue;
                else if (rez.TermValue is Boolean)
                    rez.TermValue = !(Boolean)rez.TermValue;

				return rez;
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
			}

			throw new Exception("Ошибка вычисления UnaryExpression");
		}
	}

	/// <summary>
	/// бинарное выражение
	/// </summary>
	class BinaryExpression : Expression
	{
		private Expression a, b;
		private OpCode op;
		
		public BinaryExpression(Expression a, Expression b, OpCode op)
		{
			this.a = a;
			this.b = b;
			this.op = op;
		}

		private string opCs()
		{
			try
			{
				if (op.Equals(OpCodes.Add))
					return "+";
				else if (op.Equals(OpCodes.Sub))
					return "-";
				else if (op.Equals(OpCodes.Mul))
					return "*";
				else if (op.Equals(OpCodes.Div))
					return "/";
				else if (op.Equals(OpCodes.And))
					return "&";
				else if (op.Equals(OpCodes.Or))
					return "|";
				else if (op.Equals(OpCodes.Xor))
					return "^";
				else if (op.Equals(OpCodes.Neg))
					return "!";
				else if (op.Equals(OpCodes.Prefix1))
					return "<";
				else if (op.Equals(OpCodes.Prefix2))
					return ">";
				else if (op.Equals(OpCodes.Prefix3))
					return "==";
				else if (op.Equals(OpCodes.Prefix4))
					return "!=";
				else if (op.Equals(OpCodes.Prefix5))
					return ">=";
				else if (op.Equals(OpCodes.Prefix6))
					return "<=";

				throw new Exception(string.Format("Expression.cs : BinaryExpression.opCs() : (78) : несуществующий код операции : {0}", op.Name));
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				throw ex;
			}
		}

        public override ITerm Evaluate()//Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values
		{
			try
			{
				if (op.Equals(OpCodes.Add))
					return DoAdd(a,b);
				else if (op.Equals(OpCodes.Sub))
					return DoSub(a,b);
				else if (op.Equals(OpCodes.Mul))
					return DoMul(a,b);
				else if (op.Equals(OpCodes.Div))
					return DoDiv(a, b);
				else if (op.Equals(OpCodes.And))
					return DoAnd(a, b);
				else if (op.Equals(OpCodes.Or))
					return DoOr(a, b);
				else if (op.Equals(OpCodes.Xor))
					return DoXor(a, b);
				else if (op.Equals(OpCodes.Prefix1))		// сравнение по меньше
					return DoCmpLess(a, b);
				else if (op.Equals(OpCodes.Prefix2))		// сравнение по больше
					return DoCmpMore(a, b);
				else if (op.Equals(OpCodes.Prefix3))		// сравнение по равно
					return DoEqual(a, b);
				else if (op.Equals(OpCodes.Prefix4))		// сравнение по не-равно
					return DoNoEqual(a, b);
				else if (op.Equals(OpCodes.Prefix5))		// сравнение по больше или равно
					return DoMoreOrEqual(a, b);
				else if (op.Equals(OpCodes.Prefix6))		// сравнение по меньше или равно
					return DoLessOrEqual(a, b);
				else
					throw new Exception(string.Format("Expression.cs : BinaryExpression.Evaluate() : (104) Операция не поддерживается : {0}", op.Value));
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				throw ex;
			}
		}

		#region Методы реализующие бинарные операции
        ITerm DoAdd(Expression a, Expression b)//, Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values
		{

            ITerm trez;
            try
            {

                ITerm ta = a.Evaluate();
                ITerm tb = b.Evaluate();

                trez = (ITerm)new SingleTerm();
                Single singletmp = Single.MinValue;

                if (ta is BooleanTerm)
                    singletmp = (bool)ta.TermValue ? 1 : 0;
                else
                    singletmp = (Single)ta.TermValue;

                trez.TermValue = singletmp + (Single)tb.TermValue;

                // качество
                trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }
            return trez;

		}
        /// <summary>
        /// посчитать качество
        /// </summary>
        /// <param name="varQuality1"></param>
        /// <param name="varQuality2"></param>
        /// <returns></returns>
        private ProjectCommonData.VarQuality GetVarQuality(ProjectCommonData.VarQuality varQuality1, ProjectCommonData.VarQuality varQuality2)
        {
            if ((varQuality1 == ProjectCommonData.VarQuality.vqUndefined && varQuality2 == ProjectCommonData.VarQuality.vqUndefined))
                return ProjectCommonData.VarQuality.vqUndefined;

            if ((varQuality1 == ProjectCommonData.VarQuality.vqGood && varQuality2 == ProjectCommonData.VarQuality.vqGood))
                return ProjectCommonData.VarQuality.vqGood;

            if ((varQuality1 == ProjectCommonData.VarQuality.vqHandled && varQuality2 == ProjectCommonData.VarQuality.vqGood) || (varQuality1 == ProjectCommonData.VarQuality.vqGood && varQuality2 == ProjectCommonData.VarQuality.vqHandled))
                return ProjectCommonData.VarQuality.vqCalculatedHanle;

            if ((varQuality1 == ProjectCommonData.VarQuality.vqHandled && varQuality2 == ProjectCommonData.VarQuality.vqUndefined) || (varQuality1 == ProjectCommonData.VarQuality.vqUndefined && varQuality2 == ProjectCommonData.VarQuality.vqHandled))
                return ProjectCommonData.VarQuality.vqUndefined;

            if ((varQuality1 == ProjectCommonData.VarQuality.vqGood && varQuality2 == ProjectCommonData.VarQuality.vqUndefined) || (varQuality1 == ProjectCommonData.VarQuality.vqUndefined && varQuality2 == ProjectCommonData.VarQuality.vqGood))
                return ProjectCommonData.VarQuality.vqUndefined;

            if (varQuality1 == ProjectCommonData.VarQuality.vqHandled && varQuality2 == ProjectCommonData.VarQuality.vqHandled)
                return ProjectCommonData.VarQuality.vqCalculatedHanle;

            if (varQuality1 == ProjectCommonData.VarQuality.vqCalculatedHanle && varQuality2 == ProjectCommonData.VarQuality.vqCalculatedHanle)
                return ProjectCommonData.VarQuality.vqCalculatedHanle;

            if ((varQuality1 == ProjectCommonData.VarQuality.vqCalculatedHanle && varQuality2 == ProjectCommonData.VarQuality.vqGood) || (varQuality1 == ProjectCommonData.VarQuality.vqGood && varQuality2 == ProjectCommonData.VarQuality.vqCalculatedHanle))
                return ProjectCommonData.VarQuality.vqCalculatedHanle;

            if ((varQuality1 == ProjectCommonData.VarQuality.vqCalculatedHanle && varQuality2 == ProjectCommonData.VarQuality.vqHandled) || (varQuality1 == ProjectCommonData.VarQuality.vqHandled && varQuality2 == ProjectCommonData.VarQuality.vqCalculatedHanle))
                return ProjectCommonData.VarQuality.vqCalculatedHanle;


            if ((varQuality1 == ProjectCommonData.VarQuality.vqCalculatedHanle && varQuality2 == ProjectCommonData.VarQuality.vqUndefined) || (varQuality1 == ProjectCommonData.VarQuality.vqUndefined && varQuality2 == ProjectCommonData.VarQuality.vqCalculatedHanle))
                return ProjectCommonData.VarQuality.vqCalculatedHanle;

            // ловушка для невыявленных ситуаций
            if ((varQuality1 != ProjectCommonData.VarQuality.vqUndefined && varQuality2 == ProjectCommonData.VarQuality.vqUndefined) || (varQuality1 == ProjectCommonData.VarQuality.vqUndefined && varQuality2 != ProjectCommonData.VarQuality.vqUndefined))
                return ProjectCommonData.VarQuality.vqCalculatedHanle;


            return ProjectCommonData.VarQuality.vqUndefined;
        }

        private ITerm DoSub(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new SingleTerm();
			trez.TermValue = (Single)ta.TermValue - (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoMul(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new SingleTerm();
            Single singletmp = Single.MinValue;
            try
            {
                if (ta is BooleanTerm)
                        singletmp = (bool)ta.TermValue ? 1 : 0;
                else
                    singletmp = (Single)ta.TermValue;

                trez.TermValue = singletmp * (Single)tb.TermValue;

                trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }

			return trez;
		}

        private ITerm DoDiv(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new SingleTerm();
			trez.TermValue = (Single)ta.TermValue / (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoAnd(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Boolean)ta.TermValue && (Boolean)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoOr(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Boolean)ta.TermValue || (Boolean)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoXor(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Boolean)ta.TermValue ^ (Boolean)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoCmpLess(Expression a, Expression b)//, Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Single)ta.TermValue < (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoCmpMore(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Single)ta.TermValue > (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoEqual(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			if (ta is BooleanTerm && tb is BooleanTerm)
				trez.TermValue = (Boolean)ta.TermValue == (Boolean)tb.TermValue;
			else if (tb is BooleanTerm)
            {
                /*
                 * на случай если аналог 
                 * сравниваем с лог. константой,
                 * то нужно привести константу к аналогу
                 */
                Single tmpr = 0;

                if ((bool)tb.TermValue == true)
                    tmpr = 1;

                trez.TermValue = (Single)ta.TermValue == tmpr;
            }
            else
                trez.TermValue = (Single)ta.TermValue == (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoNoEqual(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Single)ta.TermValue != (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoMoreOrEqual(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Single)ta.TermValue >= (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}

        private ITerm DoLessOrEqual(Expression a, Expression b)
		{
			ITerm ta = a.Evaluate();
			ITerm tb = b.Evaluate();

			ITerm trez = (ITerm)new BooleanTerm();
			trez.TermValue = (Single)ta.TermValue <= (Single)tb.TermValue;

            // качество
            trez.VARQuality = GetVarQuality(ta.VARQuality, tb.VARQuality);
            
            return trez;
		}
		#endregion
	}

	/// <summary>
	/// константное выражение
	/// </summary>
	class ConstExpression : Expression
	{
		ITerm term;

        public ConstExpression(Match tok, string typetok)
		{
            term = termFact.CreateTerm(tok, typetok);
		}

        public override ITerm Evaluate()
		{
            term.VARQuality = ProjectCommonData.VarQuality.vqGood;
            return term;
		}
	}

	/// <summary>
	/// выражение-переменная (идентификатор)
	/// </summary>
	class VariableExpression : Expression
	{
		Match var_token;
		KeyValuePair<string, object> val_token = new KeyValuePair<string, object>();
		ITerm term;

		public VariableExpression(Match tok, string typetok)
		{
			try
			{
                var_token = tok;
                val_token = new KeyValuePair<string, object>(tok.Value, null);
                term = termFact.CreateTerm(tok, typetok);
            }
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
		}

        public override ITerm Evaluate()
		{
			try
			{
				if (slexp_terms_values.ContainsKey(val_token.Key))
				{
					if (term is SingleTerm)
                    {
			            try
			            {
                            if (slexp_terms_values[val_token.Key].Item1 == null)
                            {
                                term.TermValue = (Single)0;
                                term.VARQuality = slexp_terms_values[val_token.Key].Item2;
                            }
                            else
                            {
                                //CultureInfo cinfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                                //// проверим десятичный разделитель
                                //if (slexp_terms_values[val_token.Key].Item1.Contains("."))
                                //    cinfo.NumberFormat.NumberDecimalSeparator = ".";
                                //else if (slexp_terms_values[val_token.Key].Item1.Contains(","))
                                //    cinfo.NumberFormat.NumberDecimalSeparator = ",";

                                //Thread.CurrentThread.CurrentCulture = cinfo;

                                term.TermValue = Convert.ToSingle(slexp_terms_values[val_token.Key].Item1);
                                term.VARQuality = slexp_terms_values[val_token.Key].Item2;
                            }
                        }
			            catch(Exception ex)
			            {
				            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			            }                    
                    }
					else if (term is BooleanTerm)
					{
						string val = Boolean.FalseString;

                        //if (slexp_terms_values[val_token.Key].Item1 == "0" || slexp_terms_values[val_token.Key].Item1 == "1")
                        //    val = slexp_terms_values[val_token.Key].Item1 == "0" ? Boolean.FalseString : Boolean.TrueString;
                        //else if (slexp_terms_values[val_token.Key].Item1 == string.Empty)
                        //    val = Boolean.FalseString;
                        //else if (slexp_terms_values[val_token.Key].Item1 == "nondef")
                        //    val = Boolean.FalseString;
                        //else
                        //    val = slexp_terms_values[val_token.Key].Item1;                        

                        string basstring = Convert.ToString(slexp_terms_values[val_token.Key].Item1);

                        if (slexp_terms_values[val_token.Key].Item1 == null)
                            val = Boolean.FalseString;
                        else if (basstring == "0" || basstring == "1")
                                val = basstring == "0" ? Boolean.FalseString : Boolean.TrueString;
                        else if (basstring == string.Empty)
                            val = Boolean.FalseString;
                        else if (basstring == "nondef")
                            val = Boolean.FalseString;
                        else
                            val = Convert.ToBoolean(slexp_terms_values[val_token.Key].Item1).ToString();     

						term.TermValue = Convert.ToBoolean(val);
                        term.VARQuality = slexp_terms_values[val_token.Key].Item2;
					}
                }
				else
					throw new Exception(string.Format("(126) Expression.cs : VariableExpression.Evaluate() : Запрос несуществующей переменной выражения : {0}.", val_token.Key));
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
			}

			return term;
		}
	}
    /// <summary>
    /// класс выражение-функция
    /// </summary>
    class FUNCExpression : Expression
    {
        /*
         * класс для вычисления значения функций
         * с синтаксисом:
         * FUNC[mul(0.0.43008,1)]
         * Механизм работы с функциями пока 
         * не доделан. Точка останова - передача 
         * строки с сигнатурой функции в конструктор 
         * FUNCExpression(string func)
         */
        private Expression a;
        ITerm rez;
        string nameFUNC = string.Empty;


        public FUNCExpression(string func)
        {
            /*
            * используем регулярное выражение для выделения 
            * подстрок вида цифры.цифры.цифры
            */
            try
            {
                //выделим имя функции
                string nameFUNCr = func.Trim().Split(new char[]{'('})[0];
                nameFUNC = nameFUNCr.Trim().Split(new char[] { '[' })[1];

                rez = termFact.CreateTerm(null, "Analog");

                Regex re = new Regex(@"[\d]+\.[\d]+\.[\d]+");
                MatchCollection mc = re.Matches(func);
                int iCountMatchs = mc.Count;

                // теперь нужно заменить dev на numrtu
                ArrayList ar = new ArrayList();
                StringBuilder sb = new StringBuilder();

                foreach (Match m in mc)
                {
                    sb.Clear();
                    sb.Append(m.Value);
                    if (!slexp_terms_values.ContainsKey(sb.ToString()))
                        slexp_terms_values.Add(sb.ToString(), new Tuple<object, ProjectCommonData.VarQuality>(string.Empty, ProjectCommonData.VarQuality.vqUndefined));
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        public override ITerm Evaluate()//Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values
        {
            ITerm trm = null;

            try
            {
                switch (nameFUNC)
                { 
                    case "Ugol":
                        return UGOL();//slexp_terms_values
                    case "Ugol_2s_style":
                        return Ugol_2s_style();//slexp_terms_values
                    default:
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 573, string.Format("{0} : {1} : Функция не поддерживается : {2} .", @"X:\Projects\00_DataServer\Expression\Expression.cs", "Evaluate()", nameFUNC));
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return trm;
        }

        private ITerm UGOL()//(Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values)
        {
            double rez_angle = 0;
            
            try
            {
                //double Uay = 0, Uax = 0, Yy = 0, Xx = 0;
                //string[] trms = new string[slexp_terms_values.Count];
                //int index = 0;

                //foreach (KeyValuePair<string, Tuple<object, ProjectCommonData.VarQuality>> kvp in slexp_terms_values)
                //{
                //    trms[index] =  kvp.Value.Item1;
                //    index++;
                //}

                //#region расчет базового вектора Ua : ugol_b = Uax - Uay
                //if (trms.Length == 1)
                //{
                //    // на случай когда оба операнда есть Ua
                //    rez.TermValue = Convert.ToSingle(rez_angle);
                //    return rez;
                //}
                //if (double.TryParse(trms[0], out Uax))
                //    if (double.TryParse(trms[1], out Uay))
                //    {
                //        rez_angle = Uax - Uay;
                //        /* корректировка результата:
                //         * iif(Control.PhaseUA - Control.PhaseUB &gt; 180,Control.PhaseUB - Control.PhaseUA+360,Control.PhaseUB - Control.PhaseUA))
                //         */
                //        if (rez_angle > 180)
                //        {
                //            rez_angle = Uay - Uax + 360;
                //        }
                //        else
                //            rez_angle = Uay - Uax;
                //    }

                //rez.TermValue = Convert.ToSingle(rez_angle);

                //// сформируем качество
                //rez.VARQuality = ProjectCommonData.VarQuality.vqGood;
                //foreach (Tuple<object, ProjectCommonData.VarQuality> kvp in slexp_terms_values.Values)
                //    if (kvp.Item2 != ProjectCommonData.VarQuality.vqGood)
                //        rez.VARQuality = ProjectCommonData.VarQuality.vqUndefined;
                //#endregion

                return rez;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            throw new Exception("Ошибка вычисления FUNCExpression");
        }
        /// <summary>
        /// расчет углов в стиле СИРИУС-2С, 2В из 
        /// </summary>
        /// <param name="slexp_terms_values"></param>
        /// <returns></returns>
        private ITerm Ugol_2s_style()//(Dictionary<string, Tuple<string, VarQuality>> slexp_terms_values)
        {
            double rez_angle = 0;

            if (slexp_terms_values.Count < 4)
                return rez;
            
            try
            {
                //double Uay = 0, Uax = 0, Yy = 0, Xx = 0;
                //string[] trms = new string[slexp_terms_values.Count];
                //int index = 0;

                //foreach (KeyValuePair<string, Tuple<object, ProjectCommonData.VarQuality>> kvp in slexp_terms_values)
                //{
                //    trms[index] = kvp.Value.Item1;
                //    index++;
                //}

                //#region расчет базового вектора Ua : ugol_b = arctg(Uay, Uax)
                //if (double.TryParse(trms[0], out Uay))
                //    if (double.TryParse(trms[1], out Uax))
                //        if (double.TryParse(trms[2], out Yy))
                //            if (double.TryParse(trms[3], out Xx))
                //            {
                //                double ugol_b = Math.Atan2(Uay, Uax);
                //                double at = Math.Atan2(Yy, Xx);
                //                double rez_radians = at - ugol_b; // результат в радианах
                //                rez_angle = rez_radians * (180 / Math.PI);

                //                // корректировка результата
                //                if (rez_angle > 180)
                //                    rez_angle -= 360;
                //                if (rez_angle < -180)
                //                    rez_angle += 360;
                //            }

                //rez.TermValue = Convert.ToSingle(rez_angle);

                //// сформируем качество
                //rez.VARQuality = ProjectCommonData.VarQuality.vqGood;
                //foreach (Tuple<object, ProjectCommonData.VarQuality> kvp in slexp_terms_values.Values)
                //    if (kvp.Item2 != ProjectCommonData.VarQuality.vqGood)
                //        rez.VARQuality = ProjectCommonData.VarQuality.vqUndefined;
                //#endregion
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
    }
}