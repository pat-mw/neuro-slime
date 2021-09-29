using System;

namespace RetinaNetworking
{
    [Serializable]
    public class NestedData
    {
        public int coomer;
        public float boomer;
        public Data zoomer;

        public NestedData(int _coomer, float _boomer, Data _zoomer)
        {
            coomer = _coomer;
            boomer = _boomer;
            zoomer = _zoomer;
        }

        public NestedData()
        {
            coomer = 0;
            boomer = 0;
            zoomer = null;
        }
    }
}

