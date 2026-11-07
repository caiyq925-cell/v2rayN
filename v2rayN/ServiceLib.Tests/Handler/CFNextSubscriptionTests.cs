using ServiceLib.Tests.CoreConfig;

namespace ServiceLib.Tests.Handler;

/// <summary>
/// Regression tests for CFNext-style subscriptions (Cloudflare preferred-IP lists):
/// plain-text VLESS share links, one per line, IPv6 literal addresses, an ECH query
/// parameter on every node and unencoded CJK remarks. Such subscriptions were reported
/// as "no reaction on desktop, failure on mobile" when imported.
/// </summary>
public class CFNextSubscriptionTests
{
    private const string Uuid = "d2b041e0-8a4f-4530-9666-a65a7547b3e9";

    private static string CfNextLine(string ipv6, string remark) =>
        $"vless://{Uuid}@[{ipv6}]:443?encryption=none&security=tls&sni=yq-net.de5.net&fp=chrome&host=yq-net.de5.net&type=ws&path=%2F{Uuid}%3Fed%3D2048&ech=cloudflare-ech.com%2Bhttps%3A%2F%2F223.5.5.5%2Fdns-query#{remark}";

    private static readonly string[] SampleLines =
    [
        CfNextLine("2606:4700::6810:7b1a", "美国·优选IP-149"),
        CfNextLine("2606:4700::6812:2fea", "美国·优选IP-025"),
        CfNextLine("2606:4700::bc72:6312", "美国·优选IP-047"),
    ];

    [Test]
    public async Task ResolveConfig_CFNextLine_ParsesAllFields()
    {
        var item = FmtHandler.ResolveConfig(SampleLines[0], out _);

        await (item != null).Should().BeTrue();
        await item!.ConfigType.Should().BeEqualTo(EConfigType.VLESS);
        await item.Address.Should().BeEqualTo("2606:4700::6810:7b1a");
        await item.Port.Should().BeEqualTo(443);
        await item.Remarks.Should().BeEqualTo("美国·优选IP-149");
        await item.Network.Should().BeEqualTo(nameof(ETransport.ws));
        await item.StreamSecurity.Should().BeEqualTo(Global.StreamSecurity);
        await item.Sni.Should().BeEqualTo("yq-net.de5.net");
        await item.EchConfigList.Should().BeEqualTo("cloudflare-ech.com+https://223.5.5.5/dns-query");
        var transport = item.GetTransportExtra();
        await transport.Host.Should().BeEqualTo("yq-net.de5.net");
        await transport.Path.Should().BeEqualTo($"/{Uuid}?ed=2048");
    }

    [Test]
    public async Task AddBatchServers_CFNextContentWithBomAndLfEndings_ImportsAll()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);
        SQLiteHelper.Instance.CreateTable<SubItem>();
        SQLiteHelper.Instance.CreateTable<ProfileItem>();

        var subId = $"sub-cfnext-{Guid.NewGuid():N}";
        await SQLiteHelper.Instance.ReplaceAsync(new SubItem
        {
            Id = subId,
            Remarks = "CFNext",
            Url = "https://yq-net.de5.net/token/sub",
        });

        // BOM prefix + LF-only line endings, as served by the CFNext worker
        var content = "\uFEFF" + string.Join("\n", SampleLines) + "\n";
        var count = await ConfigHandler.AddBatchServers(config, content, subId, true);
        var profiles = await AppManager.Instance.ProfileItems(subId) ?? [];

        await count.Should().BeEqualTo(SampleLines.Length);
        await profiles.Should().HaveCount(SampleLines.Length);
        await profiles.All(p => p.EchConfigList == "cloudflare-ech.com+https://223.5.5.5/dns-query").Should().BeTrue();
    }
}
