using System;
using System.Linq;
using System.Reflection;

namespace Loretta.CLI.Commands
{
    public class Command
    {
        public readonly String Name;
        public readonly MethodInfo Method;
        private readonly Object Instance;

        public Command ( String Name, MethodInfo Method, Object instance = null )
        {
            if ( String.IsNullOrWhiteSpace ( Name ) )
                throw new ArgumentException ( "Command name cannot be empty.", nameof ( Name ) );

            this.Name = Name;
            this.Method = Method ?? throw new ArgumentNullException ( nameof ( Method ) );
            this.Instance = instance;

            ParameterInfo[] @params = this.Method.GetParameters ( );
            for ( var i = 0; i < @params.Length; i++ )
                if (
                    // Either the attribute is not at the end
                    ( Attribute.IsDefined ( @params[i], typeof ( CommandArgumentRestAttribute ) ) && i != @params.Length - 1 )
                    // Or it has been used in an argument that
                    // isn't a string/string[]
                    || ( @params[i].ParameterType != typeof ( String[] ) && @params[i].ParameterType != typeof ( String ) )
                )
                    throw new Exception ( "[CommandArgumentRest] Should only be used on the last parameter of a Method." );
        }

        private static Object ChangeType ( String value, Type type )
        {
            return type == typeof ( String )
                ? value
                : type.IsEnum
                    ? Enum.Parse ( type, value[0].ToString ( ).ToUpper ( ) + value.Substring ( 1 ) )
                    : Convert.ChangeType ( value, type );
        }

        public Object Invoke ( String[] arguments )
        {
            ParameterInfo[] methodParams = this.Method.GetParameters ( );
            Object[] args = new Object[methodParams.Length];

            for ( var i = 0; i < methodParams.Length; i++ )
            {
                if ( i > arguments.Length - 1 )
                {
                    if ( methodParams[i].IsOptional )
                    {
                        args[i] = methodParams[i].DefaultValue;
                    }
                    else
                    {
                        throw new Exception ( $"Missing argument for {methodParams[i].Name}." );
                    }
                }
                else if ( Attribute.IsDefined ( methodParams[i], typeof ( CommandArgumentRestAttribute ) ) )
                {
                    try
                    {
                        if ( methodParams[i].ParameterType.IsArray )
                        {
                            args[i] = arguments.Skip ( i - 1 ).ToArray ( );
                        }
                        else
                        {
                            args[i] = String.Join ( " ", arguments.Skip ( i - 1 ) );
                        }
                    }
                    catch ( Exception e )
                    {
                        throw new Exception ( $"Error while attempting to read the rest value \"{methodParams[i].Name}\".", e );
                    }
                    break;
                }
                else
                {
                    try
                    {
                        args[i] = ChangeType ( arguments[i], methodParams[i].ParameterType );
                    }
                    catch ( Exception e )
                    {
                        throw new Exception ( $"Error while attempting to change the type \"{arguments[i]}\" to {methodParams[i].ParameterType.Name}.", e );
                    }
                }
            }

            return this.Method.Invoke ( this.Instance, args );
        }
    }
}
