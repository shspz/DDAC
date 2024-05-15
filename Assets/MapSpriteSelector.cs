using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpriteSelector : MonoBehaviour
{
    public Sprite spU, spD, spR, spL,
            spUD, spRL, spUR, spUL, spDR, spDL, 
            spULD, spRUL, spDRU, spLDR, spUDRL;
    public bool up,down,left,right; // 문 위치
    public int type; // 0: 일반, 1: 시작 지점, 2: 도착 지점
    public Color normalColor, enterColor, exitColor; 
    private Color mainColor;
    private SpriteRenderer rend;

    void Start() {
        rend = GetComponent<SpriteRenderer>();
        mainColor = normalColor;
        PickSprite();
        PickColor();
    }

    void PickSprite(){
        if (up){
            if (down){
                if (right){
                    if (left){
                        rend.sprite = spUDRL;
                    }else{
                        rend.sprite = spDRU;
                    }
                }else if (left){
                    rend.sprite = spULD;
                }else{
                    rend.sprite = spUD;
                }
            }else{
                if (right){
                    if (left){
                        rend.sprite = spRUL;
                    }else{
                        rend.sprite = spUR;
                    }
                }else if (left){
                    rend.sprite = spUL;
                }else{
                    rend.sprite = spU;
                }
            }
            return;
        }
        if (down){
            if (right){
                if(left){
                    rend.sprite = spLDR;
                }else{
                    rend.sprite = spDR;
                }
            }else if (left){
                rend.sprite = spDL;
            }else{
                rend.sprite = spD;
            }
            return;
        }
        if (right){
            if (left){
                rend.sprite = spRL;
            }else{
                rend.sprite = spR;
            }
        }else{
            rend.sprite = spL;
        }
    }

    void PickColor() {
        // type 변수에 따라 색상을 설정
        switch (type) {
            case 0: // 일반 방
                mainColor = normalColor;
                break;
            case 1: // 시작 지점
                mainColor = enterColor;
                break;
            case 2: // 도착 지점
                mainColor = exitColor; // 도착 지점 색상 설정
                break;
        }
        rend.color = mainColor; // SpriteRenderer의 색상 설정
    }

}
