    [System.Serializable]
    public struct PidMapping
    {
        public int jsonPid;       // PID value coming from the JSON chart (e.g., 0, 2, 3, 5)
        public int laneIndex;     // Corresponding sequential lane index (e.g., 0, 1, 2, 3)
    }