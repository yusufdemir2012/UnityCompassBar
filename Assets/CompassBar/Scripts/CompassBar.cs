using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassBar : MonoBehaviour
{
    //Camera
    public Transform MainCamera;
    private Vector3 trackObjectLastFwd;

    private float groupWidth = 0f;

    public Row CardinalDirectionRow;
    public Row DirectionDegreeRow;

    public Color32 NorthColor = Color.yellow;
    [Range(20f, 100f)]
    public float DistanceBetweenTwoRow = 45f;

    public RectTransform CompassParent;
    private RectTransform background;

    private RectTransform group0RectTransform;
    private RectTransform group1RectTransform;

    private Vector3 lastCompassParentPosition;

    private Transform trackObject;

    private void Start()
    {
        background = CompassParent.parent.GetComponent<RectTransform>();
        SetupCompass();
    }

    private void SetupCompass()
    {

        List<Transform> _instantiatedRows = new List<Transform>();

        for (int i = 0; i < 24; i++)
        {
            int _degree = 15 * (i + 1);

            bool isCardinalDirection =
                _degree == 90 ||
                _degree == 180 ||
                 _degree == 270 ||
                  _degree == 360;

            Row _obj = isCardinalDirection ? (CardinalDirectionRow) : (DirectionDegreeRow);
            Row _row = Instantiate<Row>(_obj, _obj.transform.parent);
            Vector2 v = _row.GetComponent<RectTransform>().anchoredPosition;
            v.x = i * DistanceBetweenTwoRow;
            _row.GetComponent<RectTransform>().anchoredPosition = v;



            if (_degree == 360)
            {//Cardinal Direction - N
                _row.LineImage.color = NorthColor;
                _row.DirectionText.text = "N";
            }
            else if (_degree == 90)
            {//Cardinal Direction - E
                _row.DirectionText.text = "E";
            }
            else if (_degree == 180)
            {//Cardinal Direction - S
                _row.DirectionText.text = "S";
            }
            else if (_degree == 270)
            {//Cardinal Direction - W
                _row.DirectionText.text = "W";
            }
            else
            {//Direction Degree
                _row.DirectionText.text = _degree.ToString();
            }
            _row.gameObject.SetActive(true);
            _instantiatedRows.Add(_row.transform);
        }

        Destroy(CardinalDirectionRow.gameObject);
        Destroy(DirectionDegreeRow.gameObject);

        GameObject _group0 = new GameObject("Group0", typeof(RectTransform));
        _group0.transform.SetParent(CompassParent);
        _group0.transform.localScale = Vector3.one;
        _group0.transform.localRotation = Quaternion.identity;

        Vector2 _sizeDelta = _group0.GetComponent<RectTransform>().sizeDelta;
        _sizeDelta.x = 23f * DistanceBetweenTwoRow;
        _sizeDelta.y = _group0.transform.parent.parent.GetComponent<RectTransform>().sizeDelta.y;
        _group0.GetComponent<RectTransform>().sizeDelta = _sizeDelta;
        groupWidth = _sizeDelta.x;

        Vector2 _anchoredPosition = _group0.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.y = 0f;
        _anchoredPosition.x = _sizeDelta.x / 2f;
        _group0.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;

        for (int i = _instantiatedRows.Count - 1; i >= 0; i--)
        {
            _instantiatedRows[i].SetParent(_group0.transform);
        }

        GameObject _group1 = Instantiate(_group0, _group0.transform.parent);
        _group1.name = "Group1";
        _anchoredPosition = _group1.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.x = (_anchoredPosition.x * 3f) + DistanceBetweenTwoRow;
        _group1.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;

        //Set in the midst groups in parent
        float _subtractionAmount = _group0.GetComponent<RectTransform>().sizeDelta.x;
        _anchoredPosition = _group1.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.x = _anchoredPosition.x - _subtractionAmount;
        _group1.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;
        _anchoredPosition = _group0.GetComponent<RectTransform>().anchoredPosition;
        _anchoredPosition.x = _anchoredPosition.x - _subtractionAmount;
        _group0.GetComponent<RectTransform>().anchoredPosition = _anchoredPosition;

        group0RectTransform = _group0.GetComponent<RectTransform>();
        group1RectTransform = _group1.GetComponent<RectTransform>();

        trackObject = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        trackObject.gameObject.name = "CompassBarTracker";

        Vector3 _cameraRot = MainCamera.rotation.eulerAngles;
        _cameraRot.x = 0f;
        _cameraRot.z = 0f;
        trackObject.rotation = Quaternion.Euler(_cameraRot);

        trackObjectLastFwd = trackObject.forward;

    }

    private void Update()
    {
        Vector3 _cameraRot = MainCamera.rotation.eulerAngles;
        _cameraRot.x = 0f;
        _cameraRot.z = 0f;
        trackObject.rotation = Quaternion.Euler(_cameraRot);

        var _trackObjectCurFwd = trackObject.forward;
        var _angle = Vector3.Angle(_trackObjectCurFwd, trackObjectLastFwd);
        if (_angle > 0.0001f)
        {
            bool _isNegative = Vector3.Cross(_trackObjectCurFwd, trackObjectLastFwd).y > 0;

            float _amount = (Mathf.Abs(_angle) * (groupWidth + DistanceBetweenTwoRow)) / 360f;


            Vector2 _anchoredPosition = CompassParent.anchoredPosition;
            _anchoredPosition.x = (_isNegative) ? (_anchoredPosition.x + _amount) : (_anchoredPosition.x - _amount);
            CompassParent.anchoredPosition = _anchoredPosition;
            trackObjectLastFwd = _trackObjectCurFwd;
        }
        
        ShiftingRows();
    }

    private void ShiftingRows()
    {
        if (lastCompassParentPosition.x == CompassParent.position.x)
            return;

        bool isSlidingToLeft = lastCompassParentPosition.x > CompassParent.position.x;
       
        float _dis0 = Vector2.Distance(background.position, group0RectTransform.position);
        float _dis1 = Vector2.Distance(background.position, group1RectTransform.position);
        if (_dis0 >= groupWidth)
        {
            if (group0RectTransform.position.x < background.position.x && isSlidingToLeft)
            {//Solda. Sağ tarafa ışınla
                Vector3 v = group0RectTransform.anchoredPosition;
                v.x = group1RectTransform.anchoredPosition.x + groupWidth + DistanceBetweenTwoRow;
                group0RectTransform.anchoredPosition = v;
            }
            else if (group0RectTransform.position.x > background.position.x && !isSlidingToLeft)
            {//Sağda. Sol tarafa ışınla
                Vector3 v = group0RectTransform.anchoredPosition;
                v.x = group1RectTransform.anchoredPosition.x - groupWidth - DistanceBetweenTwoRow;
                group0RectTransform.anchoredPosition = v;
            }
        }
        else if (_dis1 >= groupWidth)
        {
            if (group1RectTransform.position.x < background.position.x && isSlidingToLeft)
            {//Solda. Sağ tarafa ışınla
                Vector3 v = group1RectTransform.anchoredPosition;
                v.x = group0RectTransform.anchoredPosition.x + groupWidth + DistanceBetweenTwoRow;
                group1RectTransform.anchoredPosition = v;
            }
            else if (group1RectTransform.position.x > background.position.x && !isSlidingToLeft)
            {//Sağda. Sol tarafa ışınla
                Vector3 v = group1RectTransform.anchoredPosition;
                v.x = group0RectTransform.anchoredPosition.x - groupWidth - DistanceBetweenTwoRow;
                group1RectTransform.anchoredPosition = v;
            }
        }

        lastCompassParentPosition = CompassParent.position;

    }

}