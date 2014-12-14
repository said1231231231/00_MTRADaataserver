/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Link_NativeType_2_Discret - класс реалзизации канала преобразования типа Native в тип Discret для клиента HMI
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\LinksLib\LinksNT2PT\Link_NativeType_2_Discret.cs
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
using CommonClassesLib.CommonClasses;

namespace LinksLib.LinksNT2PT
{
    public class Link_NativeType_2_Discret : LinkNT2PTBase
    {
        public Link_NativeType_2_Discret(DataConfiguration dcNC, _01Configuration dcPL)
            : base(dcNC, dcPL)
        { 
        }
        /// <summary>
        /// вычислить значение PT по значению NT
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        protected override void ReNewPTValue()//object valueNT, ProjectCommonData.VarQuality varqualityNT, DateTime dtNT, Single trratio
        {
            //if (valueNT == null)
            //    return;
            if (TagValueNT == null)
                return;


            try
            {
                // значение
                TagValuePL = TagValueNT;
                // проверим инверсность
                if (TAGPL.IsInverseBoolValue)
                    TagValuePL = !Convert.ToBoolean(TagValuePL);
                //качество и время
                TagQualityPL = TagQualityNT;
                TimeStampPL = TimeStampNT;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
