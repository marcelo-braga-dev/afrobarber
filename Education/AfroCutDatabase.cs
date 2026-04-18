using System.Collections.Generic;
using UnityEngine;

public enum AfroCutCategory
{
    BlackPower,
    Fade,
    Braids,
    Dreads,
    Twists,
    FlatTop,
    AfroClassic,
    Contemporary,
    Traditional,
    Other
}

[System.Serializable]
public class AfroCutInfo
{
    [Header("Identificação")]
    public string cutId;
    public string cutName;
    public AfroCutCategory category;

    [Header("Linha do tempo")]
    public string decade;

    [Header("Gameplay")]
    [TextArea(2, 4)]
    public string shortGameplayDescription;
    public int rewardMoney = 25;
    public int rewardXP = 10;
    public int difficultyLevel = 1;

    [Header("Conteúdo educacional")]
    [TextArea(3, 5)]
    public string historicalSummary;

    [TextArea(5, 10)]
    public string fullHistoricalDescription;

    [TextArea(3, 5)]
    public string culturalMeaning;

    [TextArea(2, 4)]
    public string funFact;

    [Header("Visual")]
    public Sprite icon;
}

[CreateAssetMenu(fileName = "AfroCutDatabase", menuName = "AfroBarber/Education/Afro Cut Database")]
public class AfroCutDatabase : ScriptableObject
{
    [SerializeField] private List<AfroCutInfo> cuts = new List<AfroCutInfo>();

    public List<AfroCutInfo> GetAllCuts()
    {
        return cuts;
    }

    public AfroCutInfo GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return cuts.Find(c => c != null && c.cutId == id);
    }

    [ContextMenu("Carregar 6 Cortes Padrão")]
    public void LoadDefaultCuts()
    {
        cuts = new List<AfroCutInfo>
        {
            new AfroCutInfo
            {
                cutId = "black_power_classico",
                cutName = "Black Power Clássico",
                category = AfroCutCategory.AfroClassic,
                decade = "1960s–1970s",
                shortGameplayDescription = "Volume natural marcante, associado ao orgulho negro e à valorização da estética afro.",
                rewardMoney = 40,
                rewardXP = 20,
                difficultyLevel = 2,
                historicalSummary = "O Afro se tornou um dos símbolos visuais mais fortes dos movimentos Black is Beautiful e Black Power nas décadas de 1960 e 1970.",
                fullHistoricalDescription = "Embora o cabelo natural afro-texturizado exista há muito antes disso, o penteado conhecido popularmente como Afro ganhou enorme força política e cultural nas décadas de 1960 e 1970. O Afro passou a representar autoestima, consciência racial, resistência e liberdade estética.",
                culturalMeaning = "Símbolo de orgulho, identidade, resistência e valorização da beleza negra natural.",
                funFact = "A imagem de Angela Davis ajudou a transformar o Afro em um ícone político e cultural."
            },
            new AfroCutInfo
            {
                cutId = "flat_top",
                cutName = "Flat Top",
                category = AfroCutCategory.FlatTop,
                decade = "1980s–1990s",
                shortGameplayDescription = "Topo alto e marcado, ligado à estética urbana negra e ao hip-hop clássico.",
                rewardMoney = 45,
                rewardXP = 24,
                difficultyLevel = 3,
                historicalSummary = "O Flat Top se popularizou entre o fim dos anos 1980 e início dos 1990.",
                fullHistoricalDescription = "O Flat Top, especialmente em sua versão hi-top fade, ganhou enorme popularidade no fim dos anos 1980 e início dos anos 1990, tornando-se um marco visual do hip-hop.",
                culturalMeaning = "Representa expressão urbana, criatividade visual e conexão com a era de ouro do hip-hop.",
                funFact = "Virou uma das marcas visuais mais lembradas do rap do final dos anos 1980."
            },
            new AfroCutInfo
            {
                cutId = "fade_afro",
                cutName = "Fade Afro",
                category = AfroCutCategory.Fade,
                decade = "1990s–presente",
                shortGameplayDescription = "Laterais em degradê e topo afro, unindo técnica e identidade.",
                rewardMoney = 42,
                rewardXP = 22,
                difficultyLevel = 3,
                historicalSummary = "O fade afro consolidou-se como um estilo central da barbearia negra moderna.",
                fullHistoricalDescription = "A combinação do degradê lateral com o topo afro-texturizado ajudou a criar um visual preciso, moderno e muito forte na cultura urbana.",
                culturalMeaning = "Representa modernidade, precisão e evolução da estética negra na barbearia.",
                funFact = "É um dos cortes mais adaptáveis do jogo."
            },
            new AfroCutInfo
            {
                cutId = "trancas_nago",
                cutName = "Tranças Nagô",
                category = AfroCutCategory.Braids,
                decade = "Ancestral–presente",
                shortGameplayDescription = "Tranças rentes ao couro cabeludo com raízes africanas profundas.",
                rewardMoney = 50,
                rewardXP = 28,
                difficultyLevel = 4,
                historicalSummary = "Têm raízes antigas em sociedades africanas e carregam significado social e cultural.",
                fullHistoricalDescription = "As tranças rentes ao couro cabeludo atravessaram séculos entre populações negras e serviram como memória, herança e afirmação identitária.",
                culturalMeaning = "Representam ancestralidade, memória, pertencimento e resistência cultural.",
                funFact = "Em várias sociedades africanas, tranças podiam indicar idade, status e origem."
            },
            new AfroCutInfo
            {
                cutId = "dreads_locs",
                cutName = "Dreads / Locs",
                category = AfroCutCategory.Dreads,
                decade = "Antigo–presente",
                shortGameplayDescription = "Mechas compactadas ligadas à espiritualidade, identidade e resistência.",
                rewardMoney = 55,
                rewardXP = 30,
                difficultyLevel = 4,
                historicalSummary = "Locs aparecem em diferentes povos, mas ganharam forte associação moderna com identidade negra.",
                fullHistoricalDescription = "No contexto moderno, os locs ganharam força simbólica dentro de experiências negras da diáspora, inclusive em conexão com espiritualidade e afirmação identitária.",
                culturalMeaning = "Representam espiritualidade, autonomia estética e resistência à discriminação.",
                funFact = "Ganharam significado muito forte na história negra contemporânea."
            },
            new AfroCutInfo
            {
                cutId = "twists",
                cutName = "Twists",
                category = AfroCutCategory.Twists,
                decade = "Contemporâneo, com raízes tradicionais",
                shortGameplayDescription = "Torções em mechas que valorizam a textura afro e funcionam como proteção.",
                rewardMoney = 38,
                rewardXP = 18,
                difficultyLevel = 2,
                historicalSummary = "Os twists modernos fazem parte da valorização da textura afro e dos penteados protetivos.",
                fullHistoricalDescription = "Os twists ganharam força como opção prática, estética e versátil dentro do movimento de valorização do cabelo natural.",
                culturalMeaning = "Representam cuidado, versatilidade e valorização da textura natural.",
                funFact = "Podem ser usados como estilo final e também como base para outros penteados."
            }
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}