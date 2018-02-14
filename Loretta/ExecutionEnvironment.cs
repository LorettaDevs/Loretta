using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Parsing;

namespace Loretta
{
    public class ExecutionEnvironment
    {
        private IDictionary<Variable, Object> RuntimeValues = new Dictionary<Variable, Object> ( );

        public void SetVariableValue ( Variable variable, Object value )
        {
            this.RuntimeValues[variable] = value;
        }
    }
}
