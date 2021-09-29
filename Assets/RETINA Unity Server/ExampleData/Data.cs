using System;

namespace RetinaNetworking
{
    [Serializable]
    public class Data
    {
        public int coomer;
        public float boomer;
        public string zoomer;
        public enumSample loomer;

        public Data(int _coomer, float _boomer, string _zoomer, enumSample _loomer)
        {
            coomer = _coomer;
            boomer = _boomer;
            zoomer = _zoomer;
            loomer = _loomer;
        }

        public Data()
        {
            coomer = 0;
            boomer = 0;
            zoomer = null;
            loomer = 0;
        }
    }

    [Serializable]
    public enum enumSample
    {
        bob,
        charlie,
        zed
    }
}

