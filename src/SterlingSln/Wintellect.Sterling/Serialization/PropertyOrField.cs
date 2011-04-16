using System;
using System.Collections;
using System.Reflection;
#if WINPHONE7 
#else 
using System.Reflection.Emit;
#endif 

namespace Wintellect.Sterling.Serialization
{
    /// <summary>
    ///     Abstraction of property or field
    /// </summary>
    public class PropertyOrField 
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly FieldInfo _fieldInfo;

        public PropertyOrField(object infoObject)
        {
            if (infoObject == null)
            {
                throw new ArgumentNullException("infoObject");
            }

            if (infoObject is PropertyInfo)
            {
                _propertyInfo = (PropertyInfo)infoObject;
            }
            else if (infoObject is FieldInfo)
            {
                _fieldInfo = (FieldInfo)infoObject;
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type: {0}", infoObject.GetType()), "infoObject");
            }
        }

        public Type PfType
        {
            get { return _propertyInfo == null ? _fieldInfo.FieldType : _propertyInfo.PropertyType; }
        }

        public string Name
        {
            get { return _propertyInfo == null ? _fieldInfo.Name : _propertyInfo.Name; }
        }

        public Type DeclaringType
        {
            get { return _propertyInfo == null ? _fieldInfo.DeclaringType : _propertyInfo.DeclaringType; }
        }

        public object GetValue(object obj)
        {
            return _propertyInfo != null ? _propertyInfo.GetGetMethod().Invoke(obj, new object[] { }) : _fieldInfo.GetValue(obj);
        }

        public Action<object, object> Setter
        {
            get
            {
                if (_propertyInfo != null)
                {
#if WINPHONE7 
                    return (obj, prop) => _propertyInfo.GetSetMethod().Invoke(obj, new[] { prop });
#else
                    if (typeof(ICollection).IsAssignableFrom(_propertyInfo.PropertyType))
                    {
                        return (obj, prop) => _propertyInfo.GetSetMethod().Invoke(obj, new[] { prop });
                    }

                    if (_setter == null)
                    {
                        _setter = _CreateSetMethod(_propertyInfo);
                    }

                    return (obj, prop) => _setter(obj, prop);
#endif
                }

                return (obj, prop) => _fieldInfo.SetValue(obj, prop);
            }
        }

        public Func<object,object> Getter
        {
            get
            {
                if (_propertyInfo != null)
                {
#if WINPHONE7
                    return obj => _propertyInfo.GetGetMethod().Invoke(obj, new object[] { });
#else
                    if (typeof(ICollection).IsAssignableFrom(_propertyInfo.PropertyType))
                    {
                        return obj => _propertyInfo.GetGetMethod().Invoke(obj, new object[] { });
                    }

                    if (_getter == null)
                    {
                        _getter = _CreateGetMethod(_propertyInfo);
                    }

                    return obj => _getter(obj);
#endif
                }

                return obj => _fieldInfo.GetValue(obj);
            }
        }

        public override int GetHashCode()
        {
            return _propertyInfo == null ? _fieldInfo.GetHashCode() : _propertyInfo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyOrField && ((PropertyOrField) obj).PfType.Equals(PfType);
        }

        public override string ToString()
        {
            return _propertyInfo == null ? _fieldInfo.ToString() : _propertyInfo.ToString();
        }

#if WINPHONE7
#else 
        public delegate void GenericSetter(object target, object value);
        public delegate object GenericGetter(object target);

        private GenericSetter _setter;
        private GenericGetter _getter;

        private static GenericSetter _CreateSetMethod(PropertyInfo propertyInfo)
        {
            var setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;
            
            var arguments = new Type[2];
            arguments[0] = arguments[1] = typeof(object);

            var setter = new DynamicMethod(
               String.Concat("_Set", propertyInfo.Name, "_"),
               typeof(void), arguments);
            var generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            generator.Emit(propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.IsArray ? OpCodes.Castclass : OpCodes.Unbox_Any,
                           propertyInfo.PropertyType);

            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);
           
            return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
        }

        private static GenericGetter _CreateGetMethod(PropertyInfo propertyInfo)
        {           
            var getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;
            
            var arguments = new Type[1];
            arguments[0] = typeof(object);

            var getter = new DynamicMethod(
               String.Concat("_Get", propertyInfo.Name, "_"),
               typeof(object), arguments);
            var generator = getter.GetILGenerator();
            generator.DeclareLocal(typeof(object));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (!propertyInfo.PropertyType.IsClass)
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

            generator.Emit(OpCodes.Ret);
            
            return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
        }
 
#endif
    }
}