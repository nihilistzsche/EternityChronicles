using System.Collections.Generic;
using System.Linq;
using CSLog;
using DragonMUD.Network;
using DragonMUD.Utility;

// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace DragonMUD
{
    public class BaseObject : object
    {
        private readonly List<ulong> _mFlagbase;

        private readonly Dictionary<string, string> _mFlagReasons;

        private readonly Dictionary<string, int> _mFlags;

        private readonly DragonMUDProperties _mProperties;

        private int _mCurrentBitPower;

        static BaseObject()
        {
            Log.RegisterChannel("dragonmud");
        }

        public BaseObject()
        {
            _mFlagbase        = new List<ulong> { 0UL };
            _mFlags           = new Dictionary<string, int>();
            _mFlagReasons     = new Dictionary<string, string>();
            _mCurrentBitPower = 0;
            _mProperties      = new DragonMUDProperties();
        }

        public dynamic this[string propertyPath]
        {
            get => _mProperties[propertyPath];
            set => _mProperties[propertyPath] = value;
        }

        public bool IsFlagSet(string flagName)
        {
            if (!_mFlags.ContainsKey(flagName)) return false;

            var flagPower = _mFlags[flagName];

            return 1UL << (flagPower % 64) == (_mFlagbase[flagPower / 64] & (1UL << (flagPower % 64)));
        }

        public void SetFlag(string flagName)
        {
            int flagPower;

            if (_mFlags.ContainsKey(flagName))
            {
                flagPower = _mFlags[flagName];
            }
            else
            {
                _mFlags[flagName] = _mCurrentBitPower;
                if (_mFlagbase.Count() < _mCurrentBitPower / 64)
                    _mFlagbase.Add(0UL);
                flagPower = _mCurrentBitPower++;
            }

            _mFlagbase[flagPower / 64] = _mFlagbase[flagPower / 64] | (1UL << (flagPower % 64));
        }

        public void SetFlag(string flagName, string reason)
        {
            SetFlag(flagName);
            _mFlagReasons[flagName] = reason;
        }

        public string GetReasonForFlag(string flagName)
        {
            return !_mFlagReasons.ContainsKey(flagName) ? null : _mFlagReasons[flagName];
        }

        public void ClearFlag(string flagName)
        {
            if (!_mFlags.ContainsKey(flagName)) return;
            var flagPower = _mFlags[flagName];

            if (IsFlagSet(flagName))
                _mFlagbase[flagPower / 64] = _mFlagbase[flagPower / 64] ^ (1UL << (flagPower % 64));
        }

        public List<string> GetFlagKeys()
        {
            return _mFlags.Keys.ToList();
        }
    }

    public static class DragonMUDHelper
    {
        public static bool SoftRebootCheck(ConnectionCoordinator coordinator)
        {
            return !coordinator.IsFlagSet("softreboot-displayed");
        }
    }
}