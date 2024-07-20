using System;
using Newtonsoft.Json;

namespace GameplayActions
{
    [Serializable]
    public class ActionData
    {
        public GameplayActionBase Data;
        public string JsonData;
        public string Owner;
        public string Id;
        public int ActionsLeft;

        [JsonIgnore] public bool IsMine => Owner == FirebaseManager.Instance.Authentication.UserId;
    }
}

