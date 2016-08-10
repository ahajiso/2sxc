﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using ToSic.Eav;
using ToSic.Eav.DataSources;

namespace ToSic.SexyContent.DataSources
{
	[PipelineDesigner]
	public class DnnUserProfileDataSource : ExternalDataDataSource
	{
		#region Configuration-properties
		private const string PropertiesKey = "Properties";
		private const string TitleFieldKey = "TitleField";
		private const string ContentTypeKey = "ContentType";
		private const string UserIdsKey = "UserIds";
		private const string UserIdsDefaultKeyToken = "[Settings:UserIds||disabled]";
		private const string PropertiesDefaultKeyToken = "[Settings:Properties||DisplayName,Email,FirstName,LastName,Username]";
		private const string EntityTitleDefaultKeyToken = "[Settings:TitleFieldName||DisplayName]";
		private const string ContentTypeDefaultToken = "[Settings:ContentTypeName||DnnUserInfo]";

		public string UserIds
		{
			get { return Configuration[UserIdsKey]; }
			set { Configuration[UserIdsKey] = value; }
		}

		public string Properties
		{
			get { return Configuration[PropertiesKey]; }
			set { Configuration[PropertiesKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the ContentType Attribute 
		/// </summary>
		public string ContentType
		{
			get { return Configuration[ContentTypeKey]; }
			set { Configuration[ContentTypeKey] = value; }
		}

		/// <summary>
		/// Gets or sets the Name of the Title Attribute of the DNN-UserInfo
		/// </summary>
		public string TitleField
		{
			get { return Configuration[TitleFieldKey]; }
			set { Configuration[TitleFieldKey] = value; }
		}

		#endregion

		public DnnUserProfileDataSource()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(UserIdsKey, UserIdsDefaultKeyToken);
			Configuration.Add(PropertiesKey, PropertiesDefaultKeyToken);
			Configuration.Add(ContentTypeKey, ContentTypeDefaultToken);
			Configuration.Add(TitleFieldKey, EntityTitleDefaultKeyToken);
		}

		private IEnumerable<IEntity> GetList()
		{
			EnsureConfigurationIsLoaded();

			var properties = Properties.Split(',').Select(p => p.Trim()).ToArray();
			var portalId = PortalSettings.Current.PortalId;

			// read all user Profiles
			ArrayList users;
			if (UserIds == "disabled")
				users = UserController.GetUsers(portalId);
			// read user Profiles of specified UserIds
			else
			{
				var userIds = UserIds.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
				users = new ArrayList();
				foreach (var user in userIds.Select(userId => UserController.GetUserById(portalId, userId)))
					users.Add(user);
			}

			// convert Profiles to Entities
			var result = new List<IEntity>();
			foreach (UserInfo user in users)
			{
				// add Profile-Properties
				var values = new Dictionary<string, object>();
				foreach (var property in properties)
				{
					string value;
					switch (property)
					{
						case "DisplayName":
							value = user.DisplayName;
							break;
						case "Email":
							value = user.Email;
							break;
						case "FirstName":
							value = user.FirstName;
							break;
						case "LastName":
							value = user.LastName;
							break;
						case "Username":
							value = user.Username;
							break;
						default:
							value = user.Profile.GetPropertyValue(property);
							break;
					}

					values.Add(property, value);
				}

				// create Entity and add to result
				var entity = new Eav.Data.Entity(user.UserID, ContentType, values, TitleField);
				result.Add(entity);
			}

			return result;
		}
	}
}