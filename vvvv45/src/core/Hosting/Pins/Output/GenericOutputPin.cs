﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Reflection;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class GenericOutputPin<T> : Pin<T>, IGenericIO
	{
		protected INodeOut FNodeOut;
		protected bool FChanged;
		
		public GenericOutputPin(IPluginHost host, OutputAttribute attribute) : this(host, attribute, new DefaultConnectionHandler()) { }
		
		public GenericOutputPin(IPluginHost host, OutputAttribute attribute, IConnectionHandler handler)
			: base(host, attribute)
		{
			host.CreateNodeOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FNodeOut);
			
			// Register all implemented interfaces and inherited classes of T
			// to support the assignment of ISpread<Apple> output to ISpread<Fruit> input.
			var guids = new List<Guid>();
			var typeT = typeof(T);
			
			foreach (var interf in typeT.GetInterfaces())
				guids.Add(interf.GUID);
			
			while (typeT != null)
			{
				guids.Add(typeT.GUID);
				typeT = typeT.BaseType;
			}

            FNodeOut.SetSubType(guids.ToArray(), typeof(T).GetCSharpName());
			FNodeOut.SetInterface(this);
			FNodeOut.SetConnectionHandler(handler, this);
			
			base.InitializeInternalPin(FNodeOut);
		}
		
		public override T this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				FChanged = true;
			}
		}
		
		public override int SliceCount 
		{
			get 
			{ 
				return base.SliceCount; 
			}
			set 
			{ 
				base.SliceCount = value;
                FChanged = true;
			}
		}
		
		public object GetSlice(int slice)
		{
			return this[slice];
		}
		
		public override void Update()
		{
			base.Update();
			
			if (FChanged) 
			{
				if (FAttribute.SliceMode != SliceMode.Single)
					FNodeOut.SliceCount = FSliceCount;
				
				FNodeOut.MarkPinAsChanged();
			}
			
			FChanged = false;
		}
	}
}
