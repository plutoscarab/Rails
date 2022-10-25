
// Resource.cs

/*
 * Simplifies access to string resources.
 * 
 */

using System;

namespace Rails
{
	/// <summary>
	/// Summary description for Resources.
	/// </summary>
	public sealed class Resource
	{
		static System.Resources.ResourceManager resources = new System.Resources.ResourceManager("Rails.Strings", typeof(Resource).Assembly);

		private Resource()
		{
		}

		public static string GetString(string name)
		{
			string temp = resources.GetString(name);
			if (temp == null)
				throw new ArgumentException(name + " string not defined.");
			return temp;
		}
	}
}
