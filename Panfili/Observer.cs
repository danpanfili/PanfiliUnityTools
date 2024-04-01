using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Panfili.Observer{
    public class Any{
        void OnChange(Float[] Floats, UnityAction action){ foreach(Float F in Floats) F.OnChange(action); }
    }

    public class Float{
        private float val;

        public float last   = 0;
        public float delta  = 0;

        public UnityEvent Changed { get; } = new UnityEvent();
        
        public Float(float _value) { val = _value; }
        
        public float Value{
            get{ return val; }
            set{
                if (value != val){
                    last    = val;
                    val     = value;
                    delta   = val - last;

                    Changed.Invoke();
                }
            }
        }

        public void OnChange(UnityAction action){ Changed.AddListener(action); }
        public void OnChange(UnityAction[] action){ foreach(UnityAction a in action) Changed.AddListener(a); }

        public static Float operator +(Float F, float f)    { F.Value += f; return F; }
        public static Float operator -(Float F, float f)    { F.Value -= f; return F; }
        public static Float operator *(Float F, float f)    { F.Value *= f; return F; }
        public static Float operator /(Float F, float f)    { F.Value /= f; return F; }
        public static float operator +(Float F1, Float F2)  { return F1.Value + F2.Value; }
        public static float operator -(Float F1, Float F2)  { return F1.Value - F2.Value; }
        public static float operator *(Float F1, Float F2)  { return F1.Value * F2.Value; }
        public static float operator /(Float F1, Float F2)  { return F1.Value / F2.Value; }
        public static float operator +(float f, Float F)    { return f + F.Value; }
        public static float operator -(float f, Float F)    { return f - F.Value; }
        public static float operator *(float f, Float F)    { return f * F.Value; }
        public static float operator /(float f, Float F)    { return f / F.Value; }
        public static float operator +(int f, Float F)      { return f + (int) F.Value; }
        public static float operator -(int f, Float F)      { return f - (int) F.Value; }
        public static float operator *(int f, Float F)      { return f * (int) F.Value; }
        public static float operator /(int f, Float F)      { return f / (int) F.Value; }
    }

    public class Int{
        private int val;
        
        public UnityEvent Changed { get; } = new UnityEvent();
        
        public Int(int _value) { val = _value; }

        public void OnChange(UnityAction action){ Changed.AddListener(action); }
        public void OnChange(UnityAction[] action){ foreach(UnityAction a in action) Changed.AddListener(a); }
        
        public int Value{
            get{ return val; }
            set{
                if (value != val){
                    val = value;
                    Changed.Invoke();
                }
            }
        }
    }

    public class String{
        private string val;
        
        public UnityEvent Changed { get; } = new UnityEvent();
        
        public String(string value) { val = value; }

        public void OnChange(UnityAction action){ Changed.AddListener(action); }
        public void OnChange(UnityAction[] action){ foreach(UnityAction a in action) Changed.AddListener(a); }
        
        public string Value{
            get{ return val; }
            set{
                val = value;
                Changed.Invoke();
            }
        }
    }

    public class Bool{
        private bool val;
        
        public UnityEvent Changed { get; } = new UnityEvent();
        
        public Bool(bool value) { val = value; }

        public void OnChange(UnityAction action){ Changed.AddListener(action); }
        public void OnChange(UnityAction[] action){ foreach(UnityAction a in action) Changed.AddListener(a); }
        
        public bool Value{
            get{ return val; }
            set{
                val = value;
                Changed.Invoke();
            }
        }

        public static Bool operator ++(Bool B) { B.Value = !B.Value; return B; }
    }
}