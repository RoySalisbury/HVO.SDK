using System.IO;

namespace HVO.Astronomy.TheSkyX
{
    // TODO: Other methods to add here are overloads for the RA/DEC or ALT/AZ of the target.
    // TODO: We could also get this to be event driven for the asynchronous completion.  Just spin up a background thread that
    //       monitors the value and then invoke an event handler on the main TheSkyX instance.


    public sealed class ClosedLoopSlew
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal ClosedLoopSlew(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public int Execute()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX("ClosedLoopSlew.exec()", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public bool IsAsynchronous
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ClosedLoopSlew.asynchronous", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result == 1;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"ClosedLoopSlew.asynchronous = {(value ? 1 : 0)};", out var errorMessage);
            }
        }

        public double LastError
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ClosedLoopSlew.lastError", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
        }

        public bool IsComplete
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ClosedLoopSlew.isClosedLoopSlewComplete", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result == 1;
            }
        }
    }
}