using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestAction : MonoBehaviour
{
    private ActionPipeline _pipeline;
    private ActionResolver _resolver;
    [SerializeField]
    [TextArea(10, 20)]
    private string debugOutput = string.Empty;

    public IEnumerator Start()
    {
        GameService.Ins.GlobalInitialize();

        _resolver = new ActionResolver();
        _pipeline = new ActionPipeline(_resolver, new GameEffectApplier());

        // ═══════════════════════════════════════════════════════════
        // TEST: FellTree + Chop_Axe end-to-end
        // ═══════════════════════════════════════════════════════════

        // 1. Load data từ XML
        ActionDef fellTree = DatabaseThing.GetData<ActionDef>("FellTree");
        MethodDef chopAxe = DatabaseThing.GetData<MethodDef>("Chop_Axe");

        foreach(var req in fellTree.targetRequirements)
        {
            Log($"FellTree target requirement: {req.GetType().Name}");
        }

        Log("═══ FellTree + Chop_Axe Test ═══");
        Log($"Action: {fellTree.label} (effectTag: {fellTree.effectTag})");
        Log($"Method: {chopAxe.label} (executor: {chopAxe.executor.GetType().Name})");
        Log($"Method produces: {string.Join(", ", chopAxe.producesEffectTags)}");

        // 2. Tạo actor (player) và target (tree)
        DefineThing player = ThingHandler.CreateThingById("HumanDef");
        DefineThing tree = ThingHandler.CreateThingById("TreeDef");

        yield return new WaitForSeconds(0.1f); // Đợi 1 frame để các comp được Init

        // 3. Verify tree có StateComp
        Log($"Tree state: {tree.GetState()}");
        Log($"Tree HasTag IsTree: {tree.HasTag("IsTree")}");

        // 4. Tạo context
        var ctx = new ActionContext { Actor = player, Target = tree };

        // ── Trace 1: Player CÓ rìu → FellTree thành công ──
        Log("── Trace 1: Player có rìu → FellTree ──");

        // Thêm tag "HasAxeEquipped" vào player (test only, production check inventory)
        if (!player.HasTag("HasAxeEquipped"))
            player.AddTag("HasAxeEquipped");

        var validMethods = _resolver.ResolveValidMethods(fellTree, new List<MethodDef> { chopAxe }, ctx);
        Log($"Valid methods: {validMethods.Count}");

        if (validMethods.Count > 0)
        {
            var result = _pipeline.Perform(fellTree, validMethods[0], ctx);
            Log($"Result: {result.Status}");
            Log($"Tree state after: {tree.GetState()}");
            foreach (var c in result.Consequences)
                Log($"  Consequence: {c.effectTag}");
        }

        // ── Trace 2: Tree đã Stump → FellTree thất bại ──
        Log("── Trace 2: Tree đã Stump → FellTree fail ──");
        var result2 = _pipeline.Perform(fellTree, chopAxe, ctx);
        Log($"Result: {result2.Status}");
        foreach (var f in result2.FailedRequirements)
            Log($"  Failed: {f.reason_output}");

        // ── Trace 3: Player KHÔNG có rìu → Resolve fail ──

        Log("── Trace 3: Player không có rìu → Resolve fail ──");
        DefineThing player2 = ThingHandler.CreateThingById("HumanDef");
        DefineThing tree2 = ThingHandler.CreateThingById("TreeDef");
        var ctx2 = new ActionContext { Actor = player2, Target = tree2 };
        var validMethods2 = _resolver.ResolveValidMethods(fellTree, new List<MethodDef> { chopAxe }, ctx2);
        Log($"Valid methods: {validMethods2.Count} (expected 0)");

        Log("═══ Test complete ═══");
    }

    void Update() { }

    void Log(string message)
    {
        debugOutput += message + "\n";
        Debug.Log(message);
    }
}
