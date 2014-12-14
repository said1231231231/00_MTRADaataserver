/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: LinkedTags - Класс поддержки работы с расчетными тегами
 *                                                                             
 *	Файл                     : X:\Projects\00_MTRADataServer\CommonClassesLib\CommonClasses\LinkedTags.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 07.02.2011 
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
using System.Text.RegularExpressions;

namespace CommonClassesLib.CommonClasses
{
    public delegate void ChangeCalcTag(string tagid, string value, CommonClasses.ProjectCommonData.VarQuality vqual);

    public delegate void ChangeTag(string tagid/*ds.dev.tagguid*/, Tuple<string, byte[], object, CommonClasses.ProjectCommonData.VarQuality> tpl);


	public class LinkedTags
	{
			#region События
			#endregion

			#region Свойства
			#endregion

			#region public
			SortedList<string, List<ChangeCalcTag>> slListChCalcTagByTagId = new SortedList<string,List<ChangeCalcTag>>();
			#endregion

            #region конструктор(ы)
			public LinkedTags()
			{
			}
			#endregion					

			#region public-методы
			/// <summary>
			/// выделить из формулы подстроки вида ds.dev.tagguid
			/// для выполнение подписки на обновление тегов
			/// </summary>
			/// <param name="formula"></param>
			/// <param name="numrtu">устройство для подстановки вместо dev, если в запросе dev = 0 , т.е. локальное</param>
            public void ParseFormula4ExtractTagDescribe(string formula, UInt32 UniDS_GUID, UInt32 numrtu)//, ChangeCalcTag cct
			{
				/*
				 * используем регулярное выражение для выделения 
				 * подстрок вида цифры.цифры.цифры
				 */
				try
				{
					Regex re = new Regex(@"[\d]+\.[\d]+\.[\d]+");
					MatchCollection mc = re.Matches(formula);
					int iCountMatchs = mc.Count;

					// заменить ds->UniDS_GUID и dev->numrtu
					ArrayList ar = new ArrayList();
					StringBuilder sb = new StringBuilder();

					foreach (Match m in mc)
					{
						/*
						 * анализируем на локальность
						 */ 
						string[] strloc = m.Value.Split(new char[]{'.'});

						sb.Clear();						

						if (strloc[1] == "0")
							sb.Append(Regex.Replace(m.Value, @"\.[\d]+\.", string.Format(".{0}.", numrtu.ToString())));
						else
							sb.Append(m.Value);

						ar.Add(Regex.Replace(sb.ToString(), @"^[\d]+\.", string.Format("{0}.", UniDS_GUID)));
					}

					//добавляем
                    foreach (string st in ar)
                        AddTagId(st);//, cct
				}
				catch (Exception ex)
				{
					TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				}
			}

			/// <summary>
			/// функция вызываемая по изменению заказанного тега
			/// </summary>
			/// <param name="tpl"></param>
			public void ChangeAnyTag(Tuple<string, byte[]> tpl)
			{ 
			}
			#endregion

			#region public-методы реализации интерфейса xxx
			#endregion
			
			#region private-методы
			/// <summary>
			/// добавить новый тег для отслеживания и делегат для него
			/// </summary>
			/// <param name="tagid"></param>
			/// <param name="fnewval"></param>
			void AddTagId(string tagid)//, ChangeCalcTag fnewval
			{
				try
				{
					if (!slListChCalcTagByTagId.ContainsKey(tagid))
						slListChCalcTagByTagId.Add(tagid,new List<ChangeCalcTag>());
				
					//slListChCalcTagByTagId[tagid].Add(fnewval);
				}
				catch(Exception ex)
				{
					TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
				}

			}
			#endregion

			public void LinkedTagChanges2TagDelegate()
			{
                //string[] idents = new string[] { };
                //InterfaceLibrary.ITag tag = null;
                //foreach (KeyValuePair<string, List<ChangeCalcTag>> kvp in slListChCalcTagByTagId)
                //    try
                //    {
                //        idents = (kvp.Key).Split(new char[] { '.' });
                //        tag = HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[UInt32.Parse(idents[1])].GetTagByTagGUID(UInt32.Parse(idents[2]));
                //        tag.OnChangeTag += new ChangeTag(tag_OnChangeTag);

                //        tag_OnChangeTag(kvp.Key, new Tuple<string, byte[], object,  .VarQuality>(tag.ValueAsString, tag.ValueAsMemX, tag.ValueAsObject, tag.DataQualityAsEnum));
                //    }
                //    catch (Exception ex)
                //    {
                //        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                //    }
			}

			void tag_OnChangeTag(string tagid, Tuple<string, byte[], object, CommonClasses.ProjectCommonData.VarQuality> tpl)
			{
                string[] idents = new string[] { };

                try
                {
                    // определяем к какому тегу относится обновление и вызываем соответсвующие функции по списку
                    if (!slListChCalcTagByTagId.ContainsKey(tagid))
                        return;

                    foreach (ChangeCalcTag cct in slListChCalcTagByTagId[tagid])
                    {
                        cct(tagid, tpl.Item1, tpl.Item4);
                    }
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
			}
	}
}
