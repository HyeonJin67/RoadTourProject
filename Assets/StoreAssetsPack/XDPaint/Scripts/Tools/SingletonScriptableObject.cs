﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace XDPaint.Tools
{
	public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
	{
		private static T instance;

		private static readonly Dictionary<Type, string> paths = new Dictionary<Type, string>()
		{
			{ typeof(Settings), "XDPaintSettings" },
			{ typeof(BrushPresets), "XDPaintBrushPresets" }
		};

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					foreach (var key in paths.Keys)
					{
						if (typeof(T) == key)
						{
							instance = Resources.Load(paths[key]) as T;
							break;
						}
					}
				}
				return instance;
			}
		}
	}
}