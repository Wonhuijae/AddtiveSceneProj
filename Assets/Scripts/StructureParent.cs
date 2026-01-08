using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureParent : MonoBehaviour
{
    [SerializeField] GameObject triggerObject;
    static Dictionary<StructureType, StructureParent> parentRefs = new();
    [SerializeField] StructureType structureType;

    public static event Action<int> OnSwitchScene;
    public static StructureComp cachedStruct;
    public static GameObject player; 
    private static CameraController cameraController;
    // 2123 2124 2256 2388
    [SerializeField] List<StructureComp> structList = new();
    [SerializeField] Transform SpawnPos;
    public Color HoverColor;

    private void Awake()
    {
        // 자신이 관리 주체일 때만 설정
        if (StructureComp.StructureParent == null)
        {
            StructureComp.StructureParent = this;

            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            cameraController = player.GetComponent<CameraController>();
        }

        // 하위 구조물 컴포넌트들 수집
        structList = triggerObject.GetComponentsInChildren<StructureComp>().ToList();

        // 구조물 타입별로 객체 참조 저장
        parentRefs[structureType] = this;
    }

    private void OnDestroy()
    {
        // 자신이 관리 주체일 때만 해제
        if (StructureComp.StructureParent == this)
        {
            StructureComp.StructureParent = null;
            player = null;
            cameraController = null;
        }

        parentRefs.Remove(structureType);
    }

    public static StructureParent GetStructureParent(StructureType type)
    {
        if (parentRefs.TryGetValue(type, out var parent))
        {
            return parent;
        }
        return null;
    }

    // 미니어처 => 필드 이동 시엔 structID로 요청
    public void RequestMove(int structID)
    {
        Debug.Log(parentRefs.Count);

        Debug.Log($"StructureParent: RequestMove to StructID={structID}");
        cachedStruct = null;
        var filedParent = GetStructureParent(StructureType.Field);

        filedParent?.FindFieldStruct(structID);
    }

    private void FindFieldStruct(int structID)
    {
        foreach (var structure in structList)
        {
            if (structure.structID == structID && structure.structureType == StructureType.Field)
            {
                RequestMove(structure);
                OnSwitchScene?.Invoke((int)StructureType.Field);
                cachedStruct = structure;
                break;
            }
        }
    }

    public void FindNearStruct()
    {
        Collider[] hits = Physics.OverlapSphere(player.transform.position, 30f);

        foreach (var hit in hits)
        {
            var nearTower = hit.GetComponent<StructureComp>();
            if (nearTower != null)
            {
                cachedStruct = nearTower;
                break;
            }
        }

        // 못 찾았으면 null
        cachedStruct = null;
    }

    private Queue<StructureComp> moveQueue = new();
    private Coroutine moveCoroutine = null;
    private bool isProcessingQueue = false;
    public void RequestMove(StructureComp reqStruct)
    {
        if (cachedStruct == null)
        {
            FindNearStruct();
        }

        if (moveQueue.Contains(reqStruct) || cachedStruct == reqStruct || moveCoroutine != null)
            return;

        moveQueue.Enqueue(reqStruct);

        if (!isProcessingQueue)
        {
            isProcessingQueue = true;
            moveCoroutine = StartCoroutine(ProcessMoveQueue());
        }
    }

    private IEnumerator ProcessMoveQueue()
    {
        // Camera 간섭 방지
        cameraController.ignoreMovement = true;
        cameraController.enabled = false;
        cameraController.characterController.enabled = false;

        yield return null;

        while (moveQueue.Count > 0)
        {
            var target = moveQueue.Dequeue();

            player.transform.position = target.GetViewPos();

            cachedStruct?.ToggleMesh(true); // 이전 메시 활성화
            cachedStruct = target;

            target.ToggleMesh(false); // 현재 메시 비활성화
            yield return null;
        }

        isProcessingQueue = false;
        moveCoroutine = null;

        // 이동 완료 후 카메라 활성화
        cameraController.ignoreMovement = false;
        cameraController.enabled = true;
        cameraController.characterController.enabled = true;

        var ViewPoint = cachedStruct.viewPoint;
        if (ViewPoint != null)
        {
            cameraController.LookObject(ViewPoint);
        }
    }

    public void BackToMiniature()
    {
        OnSwitchScene?.Invoke((int)StructureType.Miniature);

        var miniParent = GetStructureParent(StructureType.Miniature);
        miniParent.ReturnPlayer();
    }

    private void ReturnPlayer()
    {
        player.transform.SetLocalPositionAndRotation(SpawnPos.position, SpawnPos.rotation);
    }
}
