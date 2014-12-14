/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Link_NatimeFormula - базовый класс для каналов формул
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\LinksLib\LinksNT2PT\Link_NatimeFormula.cs
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
using PresentationConfigurationLib.PresentaionConfiguration;
using Expression;

namespace LinksLib.LinksNT2PT
{
    public class Link_NatimeFormula : LinkNT2PTBase
    {
        /// <summary>
        /// строка формулы
        /// </summary>
        public string Formula { get; set; }
        /// <summary>
        /// класс построения дерева выражений
        /// </summary>
        protected Expression.Expression expr;
        /// <summary>
        /// список идентификаторов тегов
        /// </summary>
        public List<string> ListFrmlTags {get;set;}
        /// <summary>
        /// словарь значений тегов для расчета формулы
        /// </summary>
        protected Dictionary<string, Tuple<object, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality, DateTime>> DictFormulaTagsValue { get; set; }

        public Link_NatimeFormula(DataConfiguration dcNC, _01Configuration dcPL)
            : base(dcNC, dcPL)
        { 
            ListFrmlTags = new List<string>();
            DictFormulaTagsValue = new Dictionary<string, Tuple<object, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality, DateTime>>();
        }
        /// <summary>
        /// настроить ссылку на тег
        /// </summary>
        /// <returns></returns>
        public override bool CreateLink()
        {
            bool rezlink = false;
            try
            {
                foreach (string idtag in ListFrmlTags)
                {
                    string[] stidt = idtag.Split(new char[] { '.' });

                    uint idds = uint.Parse(stidt[0]);
                    uint iddev = uint.Parse(stidt[1]);
                    uint idtg = uint.Parse(stidt[2]);

                    Device DEVICEND = DCNC.GetDeviceByGUID(iddev);
                    if (DEVICEND == null)
                        throw new Exception(string.Format(@"(231) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));
                    Tag TAGND = DEVICEND.GetTagByTagGUID(idtg);
                    if (TAGND == null)
                        throw new Exception(string.Format(@"(234) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));

                    TAGND.OnChangeTagNT += ChangeTagPT;// TAGND_OnChangeTagPT;// TAGPL.linkHT2NT_OnChangeTagPT;




                    //_03Device DEVPL = DCPL.GetDeviceByGUID(iddev);
                    //if (DEVPL == null)
                    //    throw new Exception(string.Format(@"(75) ...\LinksLib\LinksNT2PT\Link_NatimeFormula.cs: CreateLink() : ошибка настройки канала."));
                    //_05Tag TGPL = DEVPL.GetTagByTagGUID(idtg);
                    //if (TGPL == null)
                    //    throw new Exception(string.Format(@"(78) ...\LinksLib\LinksNT2PT\Link_NatimeFormula.cs: : CreateLink() : ошибка настройки канала."));

                    //// установить значение
                    //if (!DictFormulaTagsValue.ContainsKey(idtag))
                    //    DictFormulaTagsValue.Add(idtag, new Tuple<object, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality, DateTime>(TGPL.TagValue, TGPL.TagQuality, TGPL.TimeStamp));
                    //else
                    //{ 
                    //}
                    // тип в глобальную таблицу
                    //if (!CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsType.ContainsKey(idtg.ToString()))
                    //    CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsType.Add(idtg.ToString(),TAGND.TypeTag);
                    //else
                    //{
                    //}

                    // установить значение
                    if (!DictFormulaTagsValue.ContainsKey(idtag))
                        DictFormulaTagsValue.Add(idtag, new Tuple<object, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality, DateTime>(TAGND.TagValue, TAGND.TagQuality, TAGND.TimeStamp));
                    else
                    {
                    }
                    //подписаться на обновление
                    //TGPL.OnChangeTagPT += ChangeTagPT;

                    // инициализировать парсер
                    //InitParser(Formula, CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsType, DEVPL.ThisDS.UniDS_GUID.ToString(), DEVPL.ObjectGUID.ToString());
                    InitParser(Formula, CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsTypeNativeLevel, DEVICEND.DataServer4ThisDevice.UniDS_GUID.ToString(), DEVICEND.DevGUID.ToString());
                }

                DEVICEPL = DCPL.GetDeviceByGUID(deviceplid);
                if (DEVICEPL == null)
                    throw new Exception(string.Format(@"(224) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));
                TAGPL = DEVICEPL.GetTagByTagGUID(tagplid);
                if (TAGPL == null)
                    throw new Exception(string.Format(@"(227) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));
                // установить коэф пересчета
                TransformationRatioPL = TAGPL.TransformationRatio;

                // расчитать значение формулы
                CalculateEcpression();

                rezlink = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rezlink;
        }
        private void ChangeTagPT(string stridtag, object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                // установить значение
                if (DictFormulaTagsValue.ContainsKey(stridtag))
                {
                    DictFormulaTagsValue[stridtag] = new Tuple<object, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality, DateTime>(value, varquality, dt);
                    // расчитать значение формулы
                    CalculateEcpression();
                }
                else
                { 
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// рассчитать значение выражения
        /// </summary>
        public virtual void CalculateEcpression()
        {
            try
            {
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
        public virtual void InitParser(string s, SortedList<string, string> slGlobalListTagsType, string UniDS_GUID, string UniDev_GUID)
        {
            /*
             * вызывается из 
             * X:\Projects\00_MTRADataServer\MTRADataServer\Fasilities\PresentationConfigurationFasility.cs
             * и переопределяется
             */
            try
            {
                throw new Exception(string.Format(@"(158) X:\Projects\00_MTRADataServer\LinksLib\LinksNT2PT\Link_NatimeFormula.cs : InitParser() : Сбой инициализации парсера формулы"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// установить значение согласно текущему режиму работы
        /// </summary>
        protected override void SetVTQBylinkMode(object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                switch (Mode)
                {
                    case LinksHT2NT.MODE.Natural:

                        TagValuePL = value;
                        TagQualityPL = varquality;
                        TimeStampPL = dt;

                        TAGPL.TagValue = TagValuePL;
                        TAGPL.TagQuality = TagQualityPL;
                        TAGPL.TimeStamp = TimeStampPL;

                        SetVTQ2Client(TagValuePL, TagQualityPL, TimeStampPL);

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
