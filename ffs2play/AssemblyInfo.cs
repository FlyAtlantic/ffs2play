/****************************************************************************
**
** Copyright (C) 2017 FSFranceSimulateur team.
** Contact: https://github.com/ffs2/ffs2play
**
** FFS2Play is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 3 of the License, or
** (at your option) any later version.
**
** FFS2Play is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** The license is as published by the Free Software
** Foundation and appearing in the file LICENSE.GPL3
** included in the packaging of this software. Please review the following
** information to ensure the GNU General Public License requirements will
** be met: https://www.gnu.org/licenses/gpl-3.0.html.
****************************************************************************/

/****************************************************************************
 * AssemblyInfo.cs is part of FF2Play project
 *
 * This class purpose a dialog interface to manage account profils
 * to connect severals FFS2Play networks servers
 * **************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ffs2play
{
	class AssemblyInfo
	{
		public AssemblyInfo(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");
			this.assembly = assembly;
		}

		private readonly Assembly assembly;

		/// <summary>
		/// Gets the title property
		/// </summary>
		public string ProductTitle
		{
			get
			{
				return GetAttributeValue<AssemblyTitleAttribute>(a => a.Title,
					   Path.GetFileNameWithoutExtension(assembly.CodeBase));
			}
		}

		/// <summary>
		/// Gets the application's version
		/// </summary>
		public string Version
		{
			get
			{
				string result = string.Empty;
				Version version = assembly.GetName().Version;
				if (version != null)
					return version.ToString();
				else
					return "1.0.0.0";
			}
		}

		/// <summary>
		/// Gets the description about the application.
		/// </summary>
		public string Description
		{
			get { return GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description); }
		}


		/// <summary>
		///  Gets the product's full name.
		/// </summary>
		public string Product
		{
			get { return GetAttributeValue<AssemblyProductAttribute>(a => a.Product); }
		}

		/// <summary>
		/// Gets the copyright information for the product.
		/// </summary>
		public string Copyright
		{
			get { return GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright); }
		}

		/// <summary>
		/// Gets the company information for the product.
		/// </summary>
		public string Company
		{
			get { return GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company); }
		}

		protected string GetAttributeValue<TAttr>(Func<TAttr,
		  string> resolveFunc, string defaultResult = null) where TAttr : Attribute
		{
			object[] attributes = assembly.GetCustomAttributes(typeof(TAttr), false);
			if (attributes.Length > 0)
				return resolveFunc((TAttr)attributes[0]);
			else
				return defaultResult;
		}
	}
}
