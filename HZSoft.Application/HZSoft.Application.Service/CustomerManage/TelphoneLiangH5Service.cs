using HZSoft.Application.Code;
using HZSoft.Application.Entity.BaseManage;
using HZSoft.Application.Entity.CustomerManage;
using HZSoft.Application.Entity.SystemManage;
using HZSoft.Application.IService.BaseManage;
using HZSoft.Application.IService.CustomerManage;
using HZSoft.Application.Service.BaseManage;
using HZSoft.Data.Repository;
using HZSoft.Util;
using HZSoft.Util.Extension;
using HZSoft.Util.WebControl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HZSoft.Application.Service.CustomerManage
{
    /// <summary>
    /// �� �� 6.1
    /// 
    /// �� ������������Ա
    /// �� �ڣ�2020-06-07 14:11
    /// �� ����ͷ�����ſ�
    /// </summary>
    public class TelphoneLiangH5Service : RepositoryFactory<TelphoneLiangH5Entity>, TelphoneLiangH5IService
    {
        private IOrganizeService orgService = new OrganizeService();
        private TelphoneLiangVipIService vipService = new TelphoneLiangVipService();
        private TelphoneVipShareIService vipShareService = new TelphoneVipShareService();
        #region ��ȡ����

        /// <summary>
        /// ����jsonƴ��sql�������������������
        /// </summary>
        /// <param name="queryJson"></param>
        /// <returns></returns>
        private string GetSql(string queryJson)
        {
            string strSql = "";
            var queryParam = queryJson.ToJObject();

            //���ݱ��
            if (!queryParam["Telphone"].IsEmpty())
            {
                string Telphone = queryParam["Telphone"].ToString();
                if (Telphone.Contains("|"))
                {
                    strSql += " and Telphone like '%" + Telphone.Replace("|", "") + "'";
                }
                else
                {
                    strSql += " and Telphone like '%" + Telphone + "%'";
                }

            }
            //����
            if (!queryParam["Grade"].IsEmpty())
            {
                string Grade = queryParam["Grade"].ToString();
                if (Grade.Contains(","))
                {
                    strSql += " and Grade in (" + Grade + ")";
                }
                else
                {
                    strSql += " and Grade = '" + Grade + "'";
                }
            }
            //����
            if (!queryParam["city"].IsEmpty())
            {
                string city = queryParam["city"].ToString();
                if (city.Contains("0000"))
                {
                    strSql += " and cityid like '" + city.Substring(0, 2) + "%'";
                }
                else if (city != "0")
                {
                    strSql += " and cityid = '" + city + "'";
                }
            }
            //��������
            if (!queryParam["City"].IsEmpty())
            {
                string City = queryParam["City"].ToString();
                strSql += " and City like '%" + City + "%'";
            }
            //�۸�����
            if (!queryParam["price"].IsEmpty())
            {
                string price = queryParam["price"].ToString();
                string[] jgqj = price.Split('-');
                if (!string.IsNullOrEmpty(jgqj[0]))
                {
                    strSql += " and price >= '" + jgqj[0] + "'";
                }
                if (!string.IsNullOrEmpty(jgqj[1]))
                {
                    strSql += " and price <= '" + jgqj[1] + "'";
                }
            }
            //�۸�����
            if (!queryParam["MaxPrice"].IsEmpty())
            {
                string MaxPrice = queryParam["MaxPrice"].ToString();
                string[] jgqj = MaxPrice.Split('-');
                if (!string.IsNullOrEmpty(jgqj[0]))
                {
                    strSql += " and MaxPrice >= '" + jgqj[0] + "'";
                }
                if (!string.IsNullOrEmpty(jgqj[1]))
                {
                    strSql += " and MaxPrice <= '" + jgqj[1] + "'";
                }
            }
            //�ų�
            if (!queryParam["except"].IsEmpty())
            {
                string except = queryParam["except"].ToString();
                strSql += " and Telphone not like '%" + except.Replace("e", "") + "%'";
            }
            //Ԣ��
            if (!queryParam["yuyi"].IsEmpty())
            {
                string yuyi = queryParam["yuyi"].ToString();
                if (yuyi == "1")
                {//1349��ˮ����
                    strSql += " and Telphone like '%1349%'";
                }
                else if (yuyi == "2")
                {//
                    strSql += " and (Telphone like '%520%' or Telphone like '%521%' or Telphone like '%1314%')";
                }
            }
            //�۳���ʶ
            if (!queryParam["SellMark"].IsEmpty())
            {
                string SellMark = queryParam["SellMark"].ToString();
                strSql += " and SellMark = " + SellMark;
            }

            //״̬����
            if (!queryParam["ExistMark"].IsEmpty())
            {
                string ExistMark = queryParam["ExistMark"].ToString();
                strSql += " and ExistMark = " + ExistMark;
            }

            //����
            if (!queryParam["Operator"].IsEmpty())
            {
                string Operator = queryParam["Operator"].ToString();
                strSql += " and Operator = '" + Operator + "'";
            }
            return strSql;
        }


        /// <summary>
        /// �ӹ��˹���ƽ̨�е�vip����
        /// </summary>
        /// <param name="vipList">vip����</param>
        /// <returns></returns>
        private string GetOtherOrg(List<string> vipList)
        {
            string shareOrg = "";
            //1.�Զ��干�����
            foreach (var item in vipList)
            {
                var sharelist = vipShareService.GetVipList(item);
                if (sharelist != null)
                {
                    foreach (var item2 in sharelist)
                    {
                        shareOrg += "'" + item2.ShareId + "',";//�����Զ���ѡ��Ĺ������
                    }
                }
            }
            return shareOrg.Trim(',');
        }
        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <param name="pagination">��ҳ</param>
        /// <param name="queryJson">��ѯ����</param>
        /// <returns>���ط�ҳ�б�</returns>
        public IEnumerable<TelphoneLiangH5Entity> GetPageList(Pagination pagination, string queryJson)
        {
            var queryParam = queryJson.ToJObject();
            string strSql = "select * from TelphoneLiangH5 where DeleteMark <> 1 and EnabledMark <> 1";
            ////������̨
            //if (!queryParam["OrganizeId"].IsEmpty())
            //{
            //    string OrganizeId = queryParam["OrganizeId"].ToString();
            //    strSql += " and OrganizeId = '" + OrganizeId + "'";
            //}
            //else
            //{
            //    if (!OperatorProvider.Provider.Current().IsSystem)
            //    {
            //        string companyId = OperatorProvider.Provider.Current().CompanyId;
            //        var organizeData = orgService.GetEntity(companyId);
            //        if (!string.IsNullOrEmpty(organizeData.OrganizeId))
            //        {
            //            //string inOrg = GetInOrg(companyId, organizeData.ParentId, organizeData.TopOrganizeId);
            //            List<string> vipList = vipService.GetVipOrgList(companyId, organizeData.ParentId, organizeData.TopOrganizeId);
            //            string inOrg = GetOtherOrg(vipList);//�Զ������ȣ�����ƽ̨���
            //            if (!string.IsNullOrEmpty(inOrg))
            //            {
            //                strSql += " and OrganizeId IN(" + inOrg + ")";
            //            }
            //        }
            //    }
            //}
            strSql += GetSql(queryJson);
            return this.BaseRepository().FindList(strSql.ToString(), pagination);
        }


        /// <summary>
        /// H5�˲�ѯ��ť��Listҳ�棬��ҳ����
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="queryJson"></param>
        /// <returns></returns>
        public IEnumerable<TelphoneLiangH5Entity> GetPageListH5LX(Pagination pagination, string queryJson)
        {
            //������sql
            string ownOrgSql = "";//Ĭ��Ϊ��

            var queryParam = queryJson.ToJObject();
            if (queryParam["OrganizeIdH5"].IsEmpty())
            {
                return null;
            }

            string organizeId = queryParam["OrganizeIdH5"].ToString();
            ownOrgSql = " and OrganizeId ='" + organizeId + "'";
            string ownWhere = ownOrgSql + GetSql(queryJson);
            
            //��������0��
            string strSql = @" SELECT * FROM (
 SELECT TelphoneID,Telphone,City,Operator,Price,MaxPrice,Grade,ExistMark,CASE ExistMark WHEN '2' THEN '��Ӫ��ɱ' WHEN '1' THEN '��Ӫ�ֿ�' ELSE '��ӪԤ��' END Description,
Package,EnabledMark,OrganizeId FROM TelphoneLiangH5 
 WHERE SellMark<>1 AND DeleteMark<>1 and EnabledMark <> 1 " + ownWhere
                + " )t ";
            
            return this.BaseRepository().FindList(strSql.ToString(), pagination);
        }


        /// <summary>
        /// ��ȡ�б�
        /// </summary>
        /// <param name="queryJson">��ѯ����</param>
        /// <returns>�����б�</returns>
        public IEnumerable<TelphoneLiangH5Entity> GetList(string queryJson)
        {
            return this.BaseRepository().IQueryable().ToList();
        }
        /// <summary>
        /// ��ȡʵ��
        /// </summary>
        /// <param name="keyValue">����ֵ</param>
        /// <returns></returns>
        public TelphoneLiangH5Entity GetEntity(int? keyValue)
        {
            return this.BaseRepository().FindEntity(keyValue);
        }
        #endregion

        #region �ύ����
        /// <summary>
        /// ɾ������
        /// </summary>
        /// <param name="keyValues">����</param>
        public void RemoveForm(string keyValues)
        {
            //this.BaseRepository().Delete(keyValues);
            IRepository db = new RepositoryFactory().BaseRepository().BeginTrans();
            if (!string.IsNullOrEmpty(keyValues))
            {
                string[] custIds = keyValues.Split(',');
                for (int i = 0; i < custIds.Length; i++)
                {
                    int? id = Convert.ToInt32(custIds[i]);
                    this.BaseRepository().Delete(id);
                }
                db.Commit();
            }
        }
        /// <summary>
        /// ��������������޸ģ�
        /// </summary>
        /// <param name="keyValue">����ֵ</param>
        /// <param name="entity">ʵ�����</param>
        /// <returns></returns>
        public void SaveForm(int? keyValue, TelphoneLiangH5Entity entity)
        {
            if (keyValue!=0)
            {
                entity.Modify(keyValue);
                this.BaseRepository().Update(entity);
            }
            else
            {
                entity.Create();
                this.BaseRepository().Insert(entity);
            }
        }

        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="dtSource">ʵ�����</param>
        /// <returns></returns>
        public string BatchAddEntity(DataTable dtSource)
        {
            int rowsCount = dtSource.Rows.Count;
            //�жϺ�������
            //string greaterMsg = IsGreater(rowsCount);
            //if (!string.IsNullOrEmpty(greaterMsg))
            //{
            //    return greaterMsg;
            //}

            IRepository db = new RepositoryFactory().BaseRepository().BeginTrans();

            int columns = dtSource.Columns.Count;
            string cf = "";
            for (int i = 0; i < rowsCount; i++)
            {
                try
                {
                    string telphone = dtSource.Rows[i][0].ToString();
                    if (telphone.Length == 11)
                    {
                        var liang_Data = db.FindEntity<TelphoneLiangH5Entity>(t => t.Telphone == telphone && t.DeleteMark != 1);//ɾ�����Ŀ����ٴε���
                        if (liang_Data != null)
                        {
                            cf += telphone + ",";
                        }

                        //����ǰ7λȷ�����к���Ӫ��
                        string Number7 = telphone.Substring(0, 7);
                        string City = "", CityId = "", Operator = "", Brand = "";
                        var TelphoneData = db.FindEntity<TelphoneDataEntity>(t => t.Number7 == Number7);
                        if (TelphoneData != null)
                        {
                            if (string.IsNullOrEmpty(TelphoneData.City))
                            {
                                return "�Ŷγ���Ϊ�գ�" + Number7;
                            }
                            else
                            {
                                City = TelphoneData.City.Replace("��", "");
                                CityId = TelphoneData.CityId;
                                Operator = TelphoneData.Operate;
                                Brand = TelphoneData.Brand;
                            }
                        }
                        else
                        {
                            return "�Ŷβ����ڣ�" + Number7;
                        }

                        //�۸�
                        if (string.IsNullOrEmpty(dtSource.Rows[i][1].ToString()))
                        {
                            return telphone + "�۸�Ϊ��";
                        }
                        decimal Price = Convert.ToDecimal(dtSource.Rows[i][1].ToString());
                        //�ɱ���
                        if (string.IsNullOrEmpty(dtSource.Rows[i][2].ToString()))
                        {
                            return telphone + "�ɱ���Ϊ��";
                        }
                        decimal MinPrice = Convert.ToDecimal(dtSource.Rows[i][2].ToString());
                        //����
                        decimal ChaPrice = Price - MinPrice;

                        //�����
                        decimal CheckPrice = Convert.ToDecimal(dtSource.Rows[i][3].ToString());

                        //���
                        string itemName = dtSource.Rows[i][4].ToString();
                        if (string.IsNullOrEmpty(itemName))
                        {
                            return telphone + "���Ϊ��";
                        }
                        string itemNCode = "";
                        var DataItemDetail = db.FindEntity<DataItemDetailEntity>(t => t.ItemName == itemName);
                        if (DataItemDetail != null)
                        {
                            itemNCode = DataItemDetail.ItemValue;
                        }
                        else
                        {
                            return "���Ͳ����ڣ�" + itemName + ",���������ֵ���ά�������͡�";
                        }

                        //�ײ�
                        string Package = dtSource.Rows[i][5].ToString();
                        //״̬
                        string existStr = dtSource.Rows[i][6].ToString();

                        if (string.IsNullOrEmpty(existStr))
                        {
                            return telphone + "�ֿ�/����/��ɱ״̬Ϊ��";
                        }
                        if (existStr != "��ɱ" && existStr != "�ֿ�" && existStr != "Ԥ��")
                        {
                            return telphone + "�ֿ�/����/��ɱ״̬��д����";
                        }
                        int existMark = 0;
                        if (existStr == "��ɱ")
                        {
                            existMark = 2;
                        }
                        else if (existStr == "�ֿ�")
                        {
                            existMark = 1;
                        }
                        else
                        {
                            existMark = 0;
                        }

                        //��ֹ�����ƹ�۴��� ������ʾ���޷��ҵ��� 7
                        decimal? MaxPrice = null;
                        if (columns == 8)
                        {
                            //�ƹ��
                            string maxPriceStr = dtSource.Rows[i][7].ToString();
                            //�����ǰ�еĵ�Ԫ�񱨴�Ҳ��ת���ʹ���
                            if (!string.IsNullOrEmpty(maxPriceStr))
                            {
                                MaxPrice = Convert.ToDecimal(maxPriceStr);
                            }
                        }


                        //�������
                        TelphoneLiangH5Entity entity = new TelphoneLiangH5Entity()
                        {
                            Telphone = telphone,
                            Price = Price,
                            MaxPrice = MaxPrice,
                            MinPrice = MinPrice,
                            ChaPrice = ChaPrice,
                            CheckPrice = CheckPrice,
                            City = City,
                            CityId = CityId,
                            Operator = Operator,
                            Brand = Brand,
                            Grade = itemNCode,
                            Package = Package,
                            ExistMark = existMark,
                            SellMark = 0,
                            DeleteMark = 0,
                            OrganizeId = OperatorProvider.Provider.Current().CompanyId,
                        };
                        entity.Create();
                        db.Insert(entity);
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.AddLog(ex.Message);
                    return ex.Message;
                }

            }
            db.Commit();
            if (cf != "")
            {
                LogHelper.AddLog("�����ظ�������룺" + cf);
                return "�����ظ�������룺" + cf;
            }
            else
            {
                return "����ɹ�";
            }

        }
        #endregion
    }
}
