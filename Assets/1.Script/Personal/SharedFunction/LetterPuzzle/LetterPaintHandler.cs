using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using XDPaint.Controllers;

// 퍼즐 칠하기 제어
public class LetterPaintHandler : MonoBehaviour
{
    [SerializeField]
    XDPaint.PaintManager paintManager;
    [SerializeField]
    RawImage paintOb;

    //연결 스크립트
    LetterDrawHandler letterDrawHandler;
    PUzzleMoveController PuzzleMove;

    [SerializeField]
    Color setColor;
    [SerializeField]
    float size;

    private void OnEnable()
    {
        ColorClick(setColor);
        OnBrushSizeSlider(size);        

        letterDrawHandler = FindObjectOfType<LetterDrawHandler>();
        PuzzleMove = FindObjectOfType<PUzzleMoveController>();

        letterDrawHandler.Draw += LetterPainting;
        letterDrawHandler.Ready += InputChange;
        letterDrawHandler.Reset += RestorePaint;

        PuzzleMove.Next += RestorePaint;
        PuzzleMove.Next += ResetTexture;
    }

    public void LetterPainting(PointerEventData eventData)
    {
        paintManager.PaintObject.OnMouseButton(eventData.position);        
    }
    public void InputChange(PointerEventData eventData)
    {
        paintManager.PaintObject.ProcessInput = true;
    }
    #region 페인팅용 함수
    //칠하기
    public void Painting()
    {
        paintManager.Brush.SetPaintTool(XDPaint.Core.PaintTool.Brush);
    }
    //그리기 다 지우기
    public void ResetTexture(PointerEventData eventData)
    {
        paintManager.PaintObject.TextureKeeper.Reset();
        paintManager.PaintObject.ClearTexture();
        paintManager.PaintObject.Render();
        paintManager.Render();
    }

    public void RestorePaint(PointerEventData eventData)
    {
        paintManager.PaintObject.OnMouseUp(eventData.position);
        paintManager.PaintObject.ProcessInput = false;
        ResetTexture(eventData);
    }

    //그리기 굵기 조정
    private void OnBrushSizeSlider(float value)
    {
        PaintController.Instance.Brush.Size = value;
        PlayerPrefs.SetFloat("XDPaintDemoBrushSize", value);
    }

    //색상 선정
    private void ColorClick(Color color)
    {
        var brushColor = PaintController.Instance.Brush.Color;
        brushColor = new Color(color.r, color.g, color.b, brushColor.a);
        PaintController.Instance.Brush.SetColor(brushColor);

        var colorString = ColorUtility.ToHtmlStringRGB(brushColor);
        PlayerPrefs.SetString("XDPaintDemoBrushColor", colorString);
    }
    #endregion
}
