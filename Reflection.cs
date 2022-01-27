
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

public static class Reflection
{
    public static Func<T, Type> GetGetterProperty<T, Type>(PropertyInfo property)
    {
        return (Func<T, Type>)
         Delegate.CreateDelegate(typeof(Func<T, Type>), null, property.GetGetMethod());
    }

    public static List<(string propName, dynamic Function)> GetGettersProperties<T>()
    {
        var properties = typeof(T).GetProperties().ToList();

        var props = new List<(string propName, dynamic Function)>();

        properties.ForEach(o =>
        {
            var methodToExec = typeof(Reflection)
            .GetMethod(nameof(GetGetterProperty))
            .MakeGenericMethod(typeof(T), o.PropertyType);

            var param = Expression.Parameter(typeof(PropertyInfo));

            var body = Expression.Call(null, methodToExec, param);

            var nowCanCall = Expression.Lambda<Func<PropertyInfo, dynamic>>(body, param).Compile();

            props.Add(new(o.Name, nowCanCall(o)));

        });

        return props;
    }


    public static IEnumerable<string> GetAllObjectPropStringValues<T>(T obj)
    {
        return GetGettersProperties<T>()
           .Select(o => $"{o.propName} : {((object)o.Function(obj))?.ToString()}" ?? "null")
           .AsEnumerable();
    }

    public static List<(string propName, dynamic Function)> GetSettersProperties<T>()
    {
        var properties = typeof(T).GetProperties().ToList();

        var props = new List<(string propName, dynamic Function)>();

        properties.ForEach(o => props.Add(new(o.Name, GetSetterProperty<T>(o))));

        return props;
    }

    public static Action<T, object> GetSetterProperty<T>(PropertyInfo prop)
    {
        var SetMethod = prop.GetSetMethod();

        if (SetMethod != null)
        {
            var valueParam = Expression.Parameter(typeof(object));
            var target = Expression.Parameter(typeof(T));

            var body = Expression.Call(target, SetMethod, Expression
            .Convert(valueParam, prop.PropertyType));

            var result = Expression.Lambda<Action<T, object>>(body, target, valueParam).Compile();
            return result;

        }

        return null;
    }

    public static void FillObject<T>(T obj, ArrayList inOrderValues = null, bool console = false)
    {
        var properties = GetSettersProperties<T>();


        if (!console)
            for (int i = 0; i < properties.Count; i++)
            {
                if (inOrderValues != null)
                    properties[i].Function(obj, inOrderValues[i]);
                else
                    break;
            }
        else
        {
            properties.ForEach(o =>
            {

                System.Console.WriteLine("Digite um valor para: " + o.propName);
                var value = Convert.ChangeType(Console.ReadLine(), typeof(T).GetProperty(o.propName).PropertyType);

                o.Function(obj, value);

            });
        }
    }

    public static void PrintAllObjectProperties<T>(T obj)
    {
        if (Environment.UserInteractive)
        {
            Console.Clear();
            GetAllObjectPropStringValues<T>(obj)
                .ToList().ForEach(o => System.Console.WriteLine(o));
        }
    }

    


}