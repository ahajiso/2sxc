﻿using System.Collections.Specialized;
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Tokens;
using ToSic.Eav.ValueProvider;

namespace ToSic.SexyContent.Engines.TokenEngine
{
    /// <summary>
    /// Copied from Form and List Module
    /// </summary>
    public class FilteredNameValueCollectionPropertyAccess : BaseValueProvider, IPropertyAccess
    {
	    readonly NameValueCollection NameValueCollection;
        public FilteredNameValueCollectionPropertyAccess(string name, NameValueCollection list)
        {
            Name = name;
            NameValueCollection = list;
        }

        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <summary>
        /// Get Property out of NameValueCollection
        /// </summary>
        /// <param name="strPropertyName"></param>
        /// <param name="strFormat"></param>
        /// <param name="formatProvider"></param>
        /// <param name="AccessingUser"></param>
        /// <param name="AccessLevel"></param>
        /// <param name="PropertyNotFound"></param>
        /// <returns></returns>
        public string GetProperty(string strPropertyName, string strFormat, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            if (NameValueCollection == null)
                return string.Empty;
            var value = NameValueCollection[strPropertyName];
            //string OutputFormat = null;
            //if (strFormat == string.Empty)
            //{
            //    OutputFormat = "g";
            //}
            //else
            //{
            //    OutputFormat = string.Empty;
            //}
            if (value != null)
            {
                var Security = new PortalSecurity();
                value = Security.InputFilter(value, PortalSecurity.FilterFlag.NoScripting);
                return Security.InputFilter(PropertyAccess.FormatString(value, strFormat), PortalSecurity.FilterFlag.NoScripting);
            }
	        PropertyNotFound = true;
	        return string.Empty;
        }

        public override string Get(string property, string format, ref bool propertyNotFound)
        {
            if (NameValueCollection == null)
                return string.Empty;
            return FormatString(NameValueCollection[property], format);
        }

        public override bool Has(string property)
        {
            throw new System.NotImplementedException();
        }
    }
}