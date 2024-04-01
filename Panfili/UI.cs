using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Panfili.UI{
    public class Window{
        string name;
        bool enable;
        Vector2 position;
        Vector2 size;

        public Window(string _name = "default", bool _enable = true){
            name = _name;
            enable = _enable;
            position = Vector2.zero;
            size = new Vector2(200,100);
        }    
        public Window(Vector2 _position, Vector2 _size, string _name = "default", bool _enable = true){
            name = _name;
            enable = _enable;
            position = _position;
            size = _size;
        }    
        public Window(Vector2 _position, string _name = "default", bool _enable = true){
            name = _name;
            enable = _enable;
            position = _position;
            size = new Vector2(200,100);
        } 

        public void Label(string text, float x = 10, float y = 10, float w = 200, float h = 20){
            GUI.Label(new Rect(position.x + x, position.y + y, size.x + w, size.y + h), text);
        }

        public void Box(float x = 10, float y = 10, float w = 200, float h = 20, float margin = 20){}

        public void Button(){}
        
        public void OnGUI(){}
    }
}
    
