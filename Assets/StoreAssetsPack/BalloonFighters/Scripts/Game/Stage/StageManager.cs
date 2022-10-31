using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Generic;



public class StageManager : MonoBehaviour
{
    readonly string STAGE = "Stage";

    static public StageManager instance
    {
        get
        {
            GameObject go = GameObject.FindGameObjectWithTag("StageManager");
            if (go == null)
            {
                Debug.Log("StageManager is not Find");
                return null;
            }
            return go.GetComponent<StageManager>();
        }
    }

    [Serializable]
    class LoadableCharacter
    {
        [SerializeField]
        public Character Prefab = null;
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    //  Member Variable
    ////////////////////////////////////////////////////////////////////////////////
    [SerializeField] 
    string               _mapFolderPath          = "Simple3dMapEditor/Resources/Mapdata";
    [SerializeField]     
    int                  _startStage             = 1;
    [SerializeField]     
    int                  _characterID            = 0;
    [SerializeField]     
    StatusUI             _statusUI               = null;
    [SerializeField]
    LoadableCharacter[]  _loadableCharacterGroup = null;

    PlayerControl    _playerController = null;
    List<AIControl>  _aiControllerList = new List<AIControl>();
    int _currentStage = 1;


    Dictionary<int,Stage> _stageDic;
    ////////////////////////////////////////////////////////////////////////////////
    //  Property
    ////////////////////////////////////////////////////////////////////////////////
    public bool IsStageClear
    {
        get
        {
            int count = _aiControllerList.Count;
            for (int i = 0; i < _aiControllerList.Count; i++)
            {
                if(_aiControllerList[i].IsDie)
                    count -=1;
            }
            
            return count <= 0;
        }
    }


    ////////////////////////////////////////////////////////////////////////////////
    //  base
    ////////////////////////////////////////////////////////////////////////////////
    void Awake()
    {
        _stageDic = new Dictionary<int, Stage>();
    }


    ////////////////////////////////////////////////////////////////////////////////
    //  Public Method
    ////////////////////////////////////////////////////////////////////////////////
    public void Initialized()
    {
        LoadStage(_startStage);

        if (_statusUI != null )
        {
             var charcterInfo = _playerController.GetComponent<CharacterInfo>();
            _statusUI.Initialize(_startStage, charcterInfo.Score, charcterInfo.Ability.Life, () => Initialized());
            _statusUI.HideGotoPopup();
        }
    }

    public void NextStage()
    {
        ClearMap(_currentStage);

        _currentStage++;

        if(_currentStage % 2 == 0 && _currentStage != 6)
            Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/simple-3d-tilemap-editor-134189");

        string filePath = string.Format("{0}/{1}/{2}{3}.dat", Application.dataPath, _mapFolderPath, STAGE, _currentStage);
        GameObject go = new GameObject(string.Format("{0}{1}", STAGE, _currentStage), typeof(Stage));
        go.transform.SetParent(transform);
        if (MapLoader.Load(filePath, go.transform))
        {
            foreach (KeyValuePair<int, List<MapObject>> mapObjDic in MapLoader.MapObjectDictionary)
            {
                foreach (MapObject mapObj in mapObjDic.Value)
                {
                    if (mapObj.tag == "Character")
                    {
                        Character character = mapObj.GetComponent<Character>();
                        var aiController    = LoadCharacter(character.ID, CharacterType.AI, mapObj, go.transform) as AIControl;
                        _aiControllerList.Add(aiController);
                    }

                    if (mapObj.tag == "PlayerSpawn")
                    {
                        PlayerControl playercontrol = FindObjectOfType<PlayerControl>();
                        if (playercontrol != null)
                        {
                            playercontrol.transform.position = mapObj.transform.position;
                            playercontrol.Clear();
                            _playerController = playercontrol;
                        }
                        else
                            _playerController = LoadCharacter(_characterID, CharacterType.Player, mapObj) as PlayerControl;
                    }
                }
            }

            if (_stageDic != null)
            {
                Stage stage = go.GetComponent<Stage>();
                if (stage != null)
                    _stageDic.Add(_currentStage, stage);
            }

            if (_statusUI != null)
                _statusUI.SetStage(_currentStage);
        }
        else
        {
            Debug.LogFormat("Can't find {0}", filePath);

            /*
            Why don't you use Simple 3d tilemap Editor?
            You can add map for your style!
            */
            if(_statusUI != null)
                _statusUI.ShowGotoPopup();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    //  Private Method
    ////////////////////////////////////////////////////////////////////////////////
    void LoadStage(int stageNumber)
    {
        Clear();

        _currentStage = stageNumber;

        string filePath = string.Format("{0}/{1}/{2}{3}.dat", Application.dataPath, _mapFolderPath, STAGE, stageNumber);
        GameObject go = new GameObject(string.Format("{0}{1}", STAGE, stageNumber), typeof(Stage));
        go.transform.SetParent(transform);
        if (MapLoader.Load(filePath, go.transform))
        {
            foreach (KeyValuePair<int, List<MapObject>> mapObjDic in MapLoader.MapObjectDictionary)
            {
                foreach (MapObject mapObj in mapObjDic.Value)
                {
                    if (mapObj.tag == "Character")
                    {
                        Character character = mapObj.GetComponent<Character>();
                        var aiController = LoadCharacter(character.ID, CharacterType.AI, mapObj, go.transform) as AIControl;
                        _aiControllerList.Add(aiController);

                    }
                    if (mapObj.tag == "PlayerSpawn")
                        _playerController = LoadCharacter(_characterID, CharacterType.Player, mapObj) as PlayerControl;
                }
            }

            if (_stageDic != null)
            {
                Stage stage = go.GetComponent<Stage>();
                if (stage != null)
                    _stageDic.Add(stageNumber, stage);
            }

            if (_statusUI != null)
                _statusUI.SetStage(stageNumber);
        }
        else
        {
            Debug.LogFormat("Can't find {0}", filePath);
        }

    }
    CharacterControl LoadCharacter(int characterID, CharacterType type, MapObject mapObject,Transform root = null)
    {
        Character prefab    = GetLoadableCharacter(characterID);
        Character character = (type == CharacterType.Player) ? Instantiate<Character>(prefab).GetComponent<Character>() : mapObject.GetComponent<Character>();

        CharacterSetup characaterContoller = ResourceLoader.LoadResource<CharacterSetup>(GameConstants.CHARACTER_CONTROLLER_PATH);
        CharacterSetup characterSetup = Instantiate(characaterContoller);
        characterSetup.name = string.Format("{0}({1})", characaterContoller.name, type.ToString());
        characterSetup.Setup(type);
        if (root != null)
            characterSetup.transform.SetParent(root);

        CharacterControl characterControl = characterSetup.GetComponent<CharacterControl>();
        character.transform.SetParent(characterControl.transform);

        CharacterInfo characterInfo = characterControl.gameObject.GetComponent<CharacterInfo>();
        if (characterInfo == null)
            characterInfo = characterControl.gameObject.AddComponent<CharacterInfo>();

        Vector3 spawnPoint = new Vector3(mapObject.transform.position.x, mapObject.transform.position.y + 0.2f, mapObject.transform.position.z);

        characterControl.Initialize(spawnPoint, _statusUI);
        characterInfo.Initialize(characterID, type, character.Ability);
        return characterControl;
  
    }

    void Clear()
    {
        ClearMap(_currentStage);
        ClearCharacter();
    }

    void ClearMap(int stageNumber)
    {
        if (_stageDic == null)
            return;

        Stage stage;
        if (_stageDic.TryGetValue(stageNumber, out stage))
            Destroy(stage.gameObject);

        _stageDic.Remove(stageNumber);
    }

    void ClearCharacter()
    {
        if (_playerController != null)
        {
            Destroy(_playerController.gameObject);
            _playerController = null;
        }

        if (_aiControllerList != null)
        {
            for (int i = 0; i < _aiControllerList.Count;i++)
            {
                if (_aiControllerList[i] == null)
                    continue;

                Destroy(_aiControllerList[i].gameObject);
            }

            _aiControllerList.Clear();
        }
    }

    Character GetLoadableCharacter(int id)
    {
        if(_loadableCharacterGroup == null)
            return null;

        for (int i = 0; i < _loadableCharacterGroup.Length; i++)
        {
            if(_loadableCharacterGroup[i].Prefab.ID == id)
                return _loadableCharacterGroup[i].Prefab;
        }

        return null;
    }
}
