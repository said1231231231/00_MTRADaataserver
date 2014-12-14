/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Link_NativeFormula_2_Analog - класс реалзизации канала преобразования формулы Native в тип Analog
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\LinksLib\LinksNT2PT\Link_NativeFormula_2_Analog.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С# 5.0, Framework 4.5                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : xx.xx.2014
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Легенда:
* 
*#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeConfigurationLib.NativeConfiguration;
using PresentationConfigurationLib.PresentaionConfiguration;
using Expression;

namespace LinksLib.LinksNT2PT
{
    public class Link_NativeFormula_2_Analog : Link_NatimeFormula
    {
        public Link_NativeFormula_2_Analog(DataConfiguration dcNC, _01Configuration dcPL)
            : base(dcNC, dcPL)
        { 
        }
        /// <summary>
        /// рассчитать значение выражения
        /// </summary>
        public override void CalculateEcpression()
        {
            ITerm rez;

            try
            {
                foreach ( string id in ListFrmlTags )  
                   if (DictFormulaTagsValue.ContainsKey(id))
                        expr.slexp_terms_values[id] =  new Tuple<object,CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality>(DictFormulaTagsValue[id].Item1,DictFormulaTagsValue[id].Item2);


                rez = expr.Evaluate();

                SetVTQBylinkMode(rez.TermValue, rez.VARQuality, DateTime.Now);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// инициализировать парсер формулы
        /// </summary>
        /// <param name="s"></param>
        /// <param name="slGlobalListTagsType">список соответствия id тега и строки его типа (Analog|Discret)</param>
        public override void InitParser(string s, SortedList<string, string> slGlobalListTagsType, string UniDS_GUID, string UniDev_GUID)
        {
            /*
             * вызывается из 
             * X:\Projects\00_MTRADataServer\MTRADataServer\Fasilities\PresentationConfigurationFasility.cs
             */
            try
            {
                Expression.Parser parser = new Expression.Parser(s, "Analog", slGlobalListTagsType, UniDS_GUID, UniDev_GUID);
                expr = parser.Parse();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
