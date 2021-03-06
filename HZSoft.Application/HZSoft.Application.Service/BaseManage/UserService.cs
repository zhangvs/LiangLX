﻿using HZSoft.Data;
using HZSoft.Data.Repository;
using HZSoft.Util;
using HZSoft.Util.WebControl;
using HZSoft.Util.Extension;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Linq;
using HZSoft.Application.Entity.BaseManage;
using HZSoft.Application.IService.BaseManage;
using HZSoft.Application.IService.AuthorizeManage;
using HZSoft.Application.Service.AuthorizeManage;
using System;
using HZSoft.Application.Code;

namespace HZSoft.Application.Service.BaseManage
{
    /// <summary>
    /// 版 本
    /// 
    /// 创建人：佘赐雄
    /// 日 期：2015.11.03 10:58
    /// 描 述：用户管理
    /// </summary>
    public class UserService : RepositoryFactory<UserEntity>, IUserService
    {
        private IAuthorizeService<UserEntity> iauthorizeservice = new AuthorizeService<UserEntity>();

        #region 获取数据
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetTable()
        {
            var strSql = new StringBuilder();
            strSql.Append(@"SELECT  u.*,
                                    d.FullName AS DepartmentName 
                            FROM    Base_User u
                                    LEFT JOIN Base_Department d ON d.DepartmentId = u.DepartmentId
                            WHERE   1=1");
            strSql.Append(" AND u.UserId <> 'System' AND u.EnabledMark = 1 AND u.DeleteMark=0");
            return this.BaseRepository().FindTable(strSql.ToString());
        }
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserEntity> GetList()
        {
            var expression = LinqExtensions.True<UserEntity>();
            expression = expression.And(t => t.UserId != "System").And(t => t.EnabledMark == 1).And(t => t.DeleteMark == 0);
            return this.BaseRepository().IQueryable(expression).OrderByDescending(t => t.CreateDate).ToList();
        }

        /// <summary>
        /// 获取靓号编辑管理员
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserEntity> GetListByOrganizeId(string organizeId,string NickName)
        {
            string strSql = "select * from Base_User  WHERE UserId!='System' AND EnabledMark=1 AND DeleteMark=0 ";
            if (!string.IsNullOrEmpty(organizeId))
            {
                strSql += " and OrganizeId in(SELECT OrganizeId FROM Base_Organize WHERE OrganizeId='" + organizeId + "' OR ParentId ='" + organizeId + "')";
            }
            strSql += "  AND WeChat = '"+ NickName + "'";
            strSql += " ORDER BY CreateDate ";

            return this.BaseRepository().FindList(strSql.ToString());
        }
        /// <summary>
        /// 获取靓号编辑管理员
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserEntity> GetUsersByOrganizeId(string organizeId)
        {
            string strSql = "select * from Base_User  WHERE UserId!='System' AND EnabledMark=1 AND DeleteMark=0 ";
            if (!string.IsNullOrEmpty(organizeId))
            {
                strSql += " and OrganizeId in(SELECT OrganizeId FROM Base_Organize WHERE OrganizeId='" + organizeId + "' OR ParentId ='" + organizeId + "')";
            }
            strSql += " ORDER BY CreateDate ";

            return this.BaseRepository().FindList(strSql.ToString());
        }
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        //public IEnumerable<UserEntity> GetListByIds()
        //{
        //    string strSql = "select * from Base_User  WHERE UserId!='System' AND EnabledMark=1 AND DeleteMark=0 ";
        //    if (!OperatorProvider.Provider.Current().IsSystem)
        //    {
        //        //string dataAutor = string.Format(OperatorProvider.Provider.Current().DataAuthorize.ReadAutorize, OperatorProvider.Provider.Current().UserId);
        //        //strSql += " and UserId in (" + dataAutor + ")";
        //        strSql += " and OrganizeId in(SELECT OrganizeId FROM Base_Organize WHERE OrganizeId='" + OperatorProvider.Provider.Current().CompanyId + "' OR ParentId ='" + OperatorProvider.Provider.Current().CompanyId + "')";
        //    }

        //    strSql += " ORDER BY CreateDate ";

        //    return this.BaseRepository().FindList(strSql.ToString());

        //}

        /// <summary>
        /// 用户列表
        /// </summary>
        /// <param name="pagination">分页</param>
        /// <param name="queryJson">查询参数</param>
        /// <returns></returns>
        public IEnumerable<UserEntity> GetPageList(Pagination pagination, string queryJson)
        {
            var expression = LinqExtensions.True<UserEntity>();
            //top 100 percent
            string strSql = "select * from Base_User where UserId!='System' AND EnabledMark=1 AND DeleteMark=0 ";
            var queryParam = queryJson.ToJObject();
            //公司主键
            if (!queryParam["organizeId"].IsEmpty())
            {
                string organizeId = queryParam["organizeId"].ToString();
                //expression = expression.And(t => t.OrganizeId.Equals(organizeId));
                strSql += " and organizeId = '" + organizeId + "'";
            }
            //部门主键
            if (!queryParam["departmentId"].IsEmpty())
            {
                string departmentId = queryParam["departmentId"].ToString();
                //expression = expression.And(t => t.DepartmentId.Equals(departmentId));
                strSql += " and departmentId = '" + departmentId + "'";
            }
            //查询条件
            if (!queryParam["condition"].IsEmpty() && !queryParam["keyword"].IsEmpty())
            {
                string condition = queryParam["condition"].ToString();
                string keyord = queryParam["keyword"].ToString();
                switch (condition)
                {
                    case "Account":            //账户
                        //expression = expression.And(t => t.Account.Contains(keyord));
                        strSql += " and Account = '" + keyord + "'";
                        break;
                    case "RealName":          //姓名
                        //expression = expression.And(t => t.RealName.Contains(keyord));
                        strSql += " and RealName = '" + keyord + "'";
                        break;
                    case "Mobile":          //手机
                        //expression = expression.And(t => t.Mobile.Contains(keyord));
                        strSql += " and Mobile = '" + keyord + "'";
                        break;
                    default:
                        break;
                }
            }

            string companyId = OperatorProvider.Provider.Current().CompanyId;
            if (!OperatorProvider.Provider.Current().IsSystem && companyId != "a5a962da-57e1-4ad4-87b2-bbdcd1b7cc92")
            {
                //string dataAutor = string.Format(OperatorProvider.Provider.Current().DataAuthorize.ReadAutorize, OperatorProvider.Provider.Current().UserId);
                //strSql += " and OrganizeId IN( select OrganizeId from Base_User where 1=1";
                //strSql += " and UserId in (" + dataAutor + ") GROUP BY OrganizeId )";
                strSql += " and OrganizeId in(SELECT OrganizeId FROM Base_Organize WHERE OrganizeId='" + OperatorProvider.Provider.Current().CompanyId + "' OR ParentId ='" + OperatorProvider.Provider.Current().CompanyId + "')";

            }
            return this.BaseRepository().FindList(strSql.ToString(), pagination);
            //expression = expression.And(t => t.UserId != "System");
            //return this.BaseRepository().FindList(expression, pagination);
        }
        /// <summary>
        /// 用户列表（ALL）
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllTable()
        {
            var strSql = new StringBuilder();
            strSql.Append(@"SELECT  u.UserId ,
                                    u.EnCode ,
                                    u.Account ,
                                    u.RealName ,
                                    u.Gender ,
                                    u.Birthday ,
                                    u.Mobile ,
                                    u.Manager ,
                                    u.OrganizeId,
                                    u.DepartmentId,
                                    o.FullName AS OrganizeName ,
                                    d.FullName AS DepartmentName ,
                                    u.RoleId ,
                                    u.DutyName ,
                                    u.PostName ,
                                    u.EnabledMark ,
                                    u.CreateDate,
                                    u.Description
                            FROM    Base_User u
                                    LEFT JOIN Base_Organize o ON o.OrganizeId = u.OrganizeId
                                    LEFT JOIN Base_Department d ON d.DepartmentId = u.DepartmentId
                            WHERE   1=1");
            strSql.Append(" AND u.UserId <> 'System' AND u.EnabledMark = 1 AND u.DeleteMark=0 order by o.FullName,d.FullName,u.RealName");
            return this.BaseRepository().FindTable(strSql.ToString());
        }
        /// <summary>
        /// 用户列表（导出Excel）
        /// </summary>
        /// <returns></returns>
        public DataTable GetExportList()
        {
            var strSql = new StringBuilder();
            strSql.Append(@"SELECT [Account]
                                  ,[RealName]
                                  ,CASE WHEN Gender=1 THEN '男' ELSE '女' END AS Gender
                                  ,[Birthday]
                                  ,[Mobile]
                                  ,[Telephone]
                                  ,u.[Email]
                                  ,[WeChat]
                                  ,[MSN]
                                  ,u.[Manager]
                                  ,o.FullName AS Organize
                                  ,d.FullName AS Department
                                  ,u.[Description]
                                  ,u.[CreateDate]
                                  ,u.[CreateUserName]
                              FROM Base_User u
                              INNER JOIN Base_Department d ON u.DepartmentId=d.DepartmentId
                              INNER JOIN Base_Organize o ON u.OrganizeId=o.OrganizeId");
            return this.BaseRepository().FindTable(strSql.ToString());
        }
        /// <summary>
        /// 用户实体
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns></returns>
        public UserEntity GetEntity(string keyValue)
        {
            return this.BaseRepository().FindEntity(keyValue);
        }
        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        public UserEntity CheckLogin(string username)
        {
            var expression = LinqExtensions.True<UserEntity>();
            expression = expression.And(t => t.Account == username);
            expression = expression.Or(t => t.Mobile == username);
            expression = expression.Or(t => t.Email == username);
            expression = expression.Or(t => t.WeChat == username);
            return this.BaseRepository().FindEntity(expression);
        }
        #endregion

        #region 验证数据
        /// <summary>
        /// 账户不能重复
        /// </summary>
        /// <param name="account">账户值</param>
        /// <param name="keyValue">主键</param>
        /// <returns></returns>
        public bool ExistAccount(string account, string keyValue)
        {
            var expression = LinqExtensions.True<UserEntity>();
            expression = expression.And(t => t.Account == account);
            if (!string.IsNullOrEmpty(keyValue))
            {
                expression = expression.And(t => t.UserId != keyValue);
            }
            return this.BaseRepository().IQueryable(expression).Count() == 0 ? true : false;
        }
        #endregion

        #region 提交数据
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="keyValue">主键</param>
        public void RemoveForm(string keyValue)
        {
            this.BaseRepository().Delete(keyValue);
        }
        /// <summary>
        /// 保存用户表单（新增、修改）
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <param name="userEntity">用户实体</param>
        /// <returns></returns>
        public string SaveForm(string keyValue, UserEntity userEntity)
        {
            IRepository db = new RepositoryFactory().BaseRepository().BeginTrans();
            try
            {
                #region 基本信息
                if (!string.IsNullOrEmpty(keyValue))
                {
                    userEntity.Modify(keyValue);
                    userEntity.Password = null;
                    db.Update(userEntity);
                }
                else
                {
                    userEntity.Create();
                    keyValue = userEntity.UserId;
                    userEntity.Secretkey = Md5Helper.MD5(CommonHelper.CreateNo(), 16).ToLower();
                    userEntity.Password = Md5Helper.MD5(DESEncrypt.Encrypt(Md5Helper.MD5(userEntity.Password, 32).ToLower(), userEntity.Secretkey).ToLower(), 32).ToLower();
                    db.Insert(userEntity);
                    
                }
                #endregion

                #region 默认添加 角色、岗位、职位
                db.Delete<UserRelationEntity>(t => t.IsDefault == 1 && t.UserId == userEntity.UserId);
                List<UserRelationEntity> userRelationEntitys = new List<UserRelationEntity>();
                //角色
                if (!string.IsNullOrEmpty(userEntity.RoleId))
                {
                    userRelationEntitys.Add(new UserRelationEntity
                    {
                        Category = 2,
                        UserRelationId = Guid.NewGuid().ToString(),
                        UserId = userEntity.UserId,
                        ObjectId = userEntity.RoleId,
                        CreateDate = DateTime.Now,
                        CreateUserId = OperatorProvider.Provider.Current().UserId,
                        CreateUserName = OperatorProvider.Provider.Current().UserName,
                        IsDefault = 1,
                    });
                }
                //岗位
                if (!string.IsNullOrEmpty(userEntity.DutyId))
                {
                    userRelationEntitys.Add(new UserRelationEntity
                    {
                        Category = 3,
                        UserRelationId = Guid.NewGuid().ToString(),
                        UserId = userEntity.UserId,
                        ObjectId = userEntity.DutyId,
                        CreateDate = DateTime.Now,
                        CreateUserId = OperatorProvider.Provider.Current().UserId,
                        CreateUserName = OperatorProvider.Provider.Current().UserName,
                        IsDefault = 1,
                    });
                }
                //职位
                if (!string.IsNullOrEmpty(userEntity.PostId))
                {
                    userRelationEntitys.Add(new UserRelationEntity
                    {
                        Category = 4,
                        UserRelationId = Guid.NewGuid().ToString(),
                        UserId = userEntity.UserId,
                        ObjectId = userEntity.PostId,
                        CreateDate = DateTime.Now,
                        CreateUserId = OperatorProvider.Provider.Current().UserId,
                        CreateUserName = OperatorProvider.Provider.Current().UserName,
                        IsDefault = 1,
                    });
                }
                db.Insert(userRelationEntitys);
                #endregion

                db.Commit();

                return keyValue;
            }
            catch (Exception)
            {
                db.Rollback();
                throw;
            }
        }
        /// <summary>
        /// 修改用户登录密码
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <param name="Password">新密码（MD5 小写）</param>
        public void RevisePassword(string keyValue, string Password)
        {
            UserEntity userEntity = new UserEntity();
            userEntity.UserId = keyValue;
            userEntity.Secretkey = Md5Helper.MD5(CommonHelper.CreateNo(), 16).ToLower();
            userEntity.Password = Md5Helper.MD5(DESEncrypt.Encrypt(Password, userEntity.Secretkey).ToLower(), 32).ToLower();
            this.BaseRepository().Update(userEntity);
        }
        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <param name="State">状态：1-启动；0-禁用</param>
        public void UpdateState(string keyValue, int State)
        {
            UserEntity userEntity = new UserEntity();
            userEntity.Modify(keyValue);
            userEntity.EnabledMark = State;
            this.BaseRepository().Update(userEntity);
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="userEntity">实体对象</param>
        public void UpdateEntity(UserEntity userEntity)
        {
            this.BaseRepository().Update(userEntity);
        }
        #endregion
    }
}
