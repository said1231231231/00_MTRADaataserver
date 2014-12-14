/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: класс для хранения raw-пакетов устройств 
 *	            (для последующей работы алгоритма записи уставок)
 *                                                                             
 *	Файл                     : X:\Projects\38_DS4BlockingPrg\uvs_MOA\PacketStorage.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 24.11.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uvs_MOA.MOA_ECU_SOURCE
{
    public delegate void PacketWithSpecificAdressIsAppearence(UInt16 address);

    public class PacketStorage
    {
        #region События
        public event PacketWithSpecificAdressIsAppearence OnPacketWithSpecificAdressIsAppearence;
		#endregion

		#region Свойства
		#endregion

		#region public
		#endregion

		#region private
        /// <summary>
        /// список соответсвия адресов пакетов и их содержимого
        /// </summary>
        SortedList<UInt16,byte[]> slPacketsByAddresses = new SortedList<UInt16,byte[]>();
        List<UInt16> lstSpecificAddress4Notification = new List<UInt16>();
		#endregion

		#region конструктор(ы)
		#endregion
					
		#region public-методы реализации интерфейса ...
		#endregion

		#region public-методы
        /// <summary>
        /// добавление пакета в список
        /// пакетов, полученных от ФК
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="arr"></param>
        public void AddPacket(UInt16 address, byte[] arr)
        {
            try
			{
                byte[] tmp = new byte[arr.Length];

                Buffer.BlockCopy(arr, 0, tmp, 0, arr.Length);

                if (slPacketsByAddresses.ContainsKey(address))
                    slPacketsByAddresses[address] = tmp;
                else
                    slPacketsByAddresses.Add(address, tmp);

                /*
                 * проверим есть ли адрес пакета в списке
                 * рассылки извещений
                 */ 
                 if(lstSpecificAddress4Notification.Contains(address))
                    if (OnPacketWithSpecificAdressIsAppearence != null)
                        OnPacketWithSpecificAdressIsAppearence(address);
			}
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
        }
        /// <summary>
        /// возвратить массив байт по его адресу
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[] GetPacketByAddress(ushort address)
        {
            byte[] rez = null;
            try
            {
                if (slPacketsByAddresses.ContainsKey(address))
                    rez = slPacketsByAddresses[address];
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// удаление пакета из списка
        /// пакетов, полученных от ФК
        /// </summary>
        /// <param name="adress"></param>
        /// <param name="arr"></param>
        public void RemovePacket(UInt16 address)
        {
			try
			{
                if (slPacketsByAddresses.ContainsKey(address))
                    slPacketsByAddresses.Remove(address);
            }
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
        }
        /// <summary>
        /// добавить адрес пакета для события извещения о появлении пакета
        /// </summary>
        /// <param name="address"></param>
        public void AddPacket4NotificationAboutAppearencePacketWithSpecificAdress(UInt16 address)
        {
            try
			{
                if (!lstSpecificAddress4Notification.Contains(address))
                    lstSpecificAddress4Notification.Add(address);
            }
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
        }
        /// <summary>
        /// убрать адрес пакета для события извещения о появлении пакета
        /// </summary>
        /// <param name="address"></param>
        public void DeletePacket4NotificationAboutAppearencePacketWithSpecificAdress(UInt16 address)
        {
            try
            {
                if (lstSpecificAddress4Notification.Contains(address))
                    lstSpecificAddress4Notification.Remove(address);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            
        }
        #endregion

		#region private-методы
		#endregion

    }
}
