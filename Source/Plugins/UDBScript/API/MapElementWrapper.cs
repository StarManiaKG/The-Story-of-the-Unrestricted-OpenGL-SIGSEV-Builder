﻿#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	internal abstract class MapElementWrapper
	{
		#region ================== Variables

		private MapElement element;

		#endregion

		#region ================== Properties

		/// <summary>
		/// UDMF fields. It's an object with the fields as properties.
		/// ```
		/// s.fields.comment = 'This is a comment';
		/// s.fields['comment'] = 'This is a comment'; // Also  works
		/// s.fields.xscalefloor = 2.0;
		/// t.fields.score = 100;
		/// ```
		/// It is also possible to define new fields:
		/// ```
		/// s.fields.user_myboolfield = true;
		/// ```
		/// There are some restrictions, though:
		/// 
		/// * it only works for fields that are not in the base UDMF standard, since those are handled directly in the respective class
		/// * it does not work for flags. While they are technically also UDMF fields, they are handled in the `flags` field of the respective class (where applicable)
		/// * JavaScript does not distinguish between integer and floating point numbers, it only has floating point numbers (of double precision). For fields where UDB knows that they are integers this it not a problem, since it'll automatically convert the floating point numbers to integers (dropping the fractional part). However, if you need to specify an integer value for an unknown or custom field you have to work around this limitation:
		/// Version 5 and later:
		/// You can use a `BigInt`. This is done by appending a `n` to the number. Note that this is just a convenient way to define whole numbers, it still only supports 32 bit integers:
		/// ```
		/// s.fields.user_myintfield = 25n; // Sets the 'user_myintfield' field to an integer value of 25
		/// ```
		/// In version 4 and earlier you have to use the `UniValue` class:
		/// ```
		/// s.fields.user_myintfield = new UDB.UniValue(0, 25); // Sets the 'user_myintfield' field to an integer value of 25
		/// ```
		/// To remove a field you have to assign `null` to it:
		/// ```
		/// s.fields.user_myintfield = null;
		/// ```
		/// </summary>
		public ExpandoObject fields
		{
			get
			{
				if (element.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException(element.GetType() + " is disposed, the fields property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				foreach (KeyValuePair<string, UniValue> f in element.Fields)
					o.Add(f.Key, f.Value.Value);

				AddManagedFields(o);

				// Create event that gets called when a property is changed. This sets the flag
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) =>	{
					PropertyChangedEventArgs pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					string pname = pcea.PropertyName;
					object newvalue = null;

					// If this property was changed, but doesn't exist, then it was deleted and we should not do anything
					if (!so.ContainsKey(pname))
						return;

					if (pname != pname.ToLowerInvariant())
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("UDMF field names must be lowercase");

					// Make sure the given field is not a flag (at least for now)
					if((element is Linedef && General.Map.Config.LinedefFlags.Keys.Contains(pname)) ||
						(element is Sidedef && General.Map.Config.SidedefFlags.Keys.Contains(pname)) ||
						(element is Sector && General.Map.Config.SectorFlags.Keys.Contains(pname)) ||
						(element is Thing && General.Map.Config.ThingFlags.Keys.Contains(pname))
					)
					{
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("You are trying to modify a flag through the UDMF fields. Please use the 'flags' property instead.");
					}

					if (element.Fields.ContainsKey(pname)) // Field already exists
					{
						if (so[pname] != null)
						{
							object oldvalue = element.Fields[pname].Value;
							object proposedvalue;

							if (so[pname] is UniValue)
								proposedvalue = BuilderPlug.Me.GetConvertedUniValue((UniValue)so[pname]);
							else if(so[pname] is BigInteger pv)
							{
								// Manually check the range, so that we can show a more useful error
								if (pv > int.MaxValue)
									throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException($"Value {pv} for UDMF field \"{pname}\" is too big. Maximum value is {int.MaxValue}");

								if (pv < int.MinValue)
									throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException($"Value {pv} for UDMF field \"{pname}\" is too small. Minimum value is {int.MinValue}");

								proposedvalue = (int)pv;
							}
							else
								proposedvalue = so[pname];

							if (proposedvalue is double && (oldvalue is int || oldvalue is double))
							{
								if (oldvalue is int)
									newvalue = Convert.ToInt32((double)proposedvalue);
								else
									newvalue = (double)proposedvalue;
							}
							else if(proposedvalue is int && (oldvalue is int || oldvalue is double))
							{
								if (oldvalue is int)
									newvalue = (int)proposedvalue;
								else
									newvalue = Convert.ToDouble((int)proposedvalue);
							}
							else if (proposedvalue is string && oldvalue is string)
							{
								newvalue = (string)proposedvalue;
							}
							else if (proposedvalue is bool && oldvalue is bool)
							{
								newvalue = (bool)proposedvalue;
							}
							else
								throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("UDMF field '" + pname + "' is of incompatible type for value " + so[pname]);
						}
					}
					else // Property name doesn't exist yet
					{
						List<UniversalFieldInfo> ufis = null;

						// Get known UDMF fields for the element type
						if (element is Sector)
							ufis = General.Map.Config.SectorFields;
						else if (element is Thing)
							ufis = General.Map.Config.ThingFields;
						else if (element is Linedef)
							ufis = General.Map.Config.LinedefFields;
						else if (element is Sidedef)
							ufis = General.Map.Config.SidedefFields;
						else if (element is Vertex)
							ufis = General.Map.Config.VertexFields;
						else
							throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("element is of unsupported type");

						// Check if there's a known field with the name
						UniversalFieldInfo ufi = ufis.Where(e => e.Name == pname).FirstOrDefault();

						if (ufi == null) // Not a known UniversalField, so no further checks needed
						{
							if (so[pname] is UniValue) // Special handling when a UniValue is given. This is important when the user wants to supply an int, since they don't exist in JS
								newvalue = BuilderPlug.Me.GetConvertedUniValue((UniValue)so[pname]);
							else
								newvalue = so[pname];
						}
						else
						{
							if (so[pname] == null)
								newvalue = null;
							else if (so[pname] is double && ufi.Default is double)
								newvalue = (double)so[pname];
							else if (so[pname] is double && ufi.Default is int)
								newvalue = Convert.ToInt32((double)so[pname]);
							else if (so[pname] is bool && ufi.Default is bool)
								newvalue = (bool)so[pname];
							else if (so[pname] is string && ufi.Default is string)
								newvalue = (string)so[pname];
							else
								throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("UDMF field '" + pname + "' is of incompatible type for value " + so[pname]);
						}
					}

					element.Fields.BeforeFieldsChange();

					if (ProcessManagedField(element.Fields, pname, newvalue))
					{
						if (newvalue == null) // Remove the field when null was passed
							so.Remove(pname);
					}
					else
					{
						if (newvalue == null) // Remove the field when null was passed
						{
							element.Fields.Remove(pname);
							so.Remove(pname);
						}
						else if (newvalue is double)
							UniFields.SetFloat(element.Fields, pname, (double)newvalue);
						else if (newvalue is int)
							UniFields.SetInteger(element.Fields, pname, (int)newvalue);
						else if (newvalue is string)
							UniFields.SetString(element.Fields, pname, (string)newvalue, null);
						else if (newvalue is bool)
							element.Fields[pname] = new UniValue(UniversalType.Boolean, (bool)newvalue);
						else if (newvalue is BigInteger nv)
						{
							// Manually check the range, so that we can show a more useful error
							if(nv > int.MaxValue)
								throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException($"Value {nv} for UDMF field \"{pname}\" is too big. Maximum value is {int.MaxValue}");

							if (nv < int.MinValue)
								throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException($"Value {nv} for UDMF field \"{pname}\" is too small. Minimum value is {int.MinValue}");

							UniFields.SetInteger(element.Fields, pname, (int)nv);
						}
					}

					AfterFieldsUpdate();
				});

				return eo;
			}
		}

		#endregion

		#region ================== Constructors

		internal MapElementWrapper(MapElement element)
		{
			this.element = element;
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Called after the UDMF fields were updated, so other changes can be made to the map element, if necessary.
		/// </summary>
		internal abstract void AfterFieldsUpdate();

		/// <summary>
		/// Adds fields to the dictionary that are handled directly by UDB, but changing them is emulated through the UDMF fields.
		/// </summary>
		/// <param name="fields">UniFields of the map element</param>
		internal virtual void AddManagedFields(IDictionary<string, object> fields) { }

		/// <summary>
		/// Processed a managed UDMF field, setting the managed value to what the user set in the UDMF field.
		/// </summary>
		/// <param name="fields">UniFields of the map element</param>
		/// <param name="pname">field property name</param>
		/// <param name="newvalue">field value</param>
		/// <returns>true if the field needed to be processed, false if it didn't</returns>
		internal virtual bool ProcessManagedField(UniFields fields, string pname, object newvalue) { return false; }

		#endregion
	}
}
