using System.Collections.Generic;
using MementoMori.Dialogue;
using UnityEditor;
using UnityEngine;

namespace MementoMori.EditorTools
{
    /// <summary>Creates the dialogue assets from the approved GDD text. Safe to run repeatedly.</summary>
    public static class DialogueAssetsBaker
    {
        const string Root = "Assets/Resources/Dialogue";

        [MenuItem("Memento Mori/Build/Dialogue assets from GDD")]
        public static void Bake()
        {
            Ensure("DLG_ROOM_OPENING", L("Melantha", "Terceira noite sem sonho."), L("Melantha", "Ou terceira manhã sem lembrança."), L("Melantha", "A diferença deveria me tranquilizar."));
            Ensure("DLG_ROOM_BOWL_01", L("Melantha", "Lavei ontem."), L("Melantha", "Não havia motivo."), L("Melantha", "Talvez esse tenha sido o motivo."));
            Ensure("DLG_ROOM_BOWL_02", L("Melantha", "Hábito e esperança usam os mesmos gestos."));
            Ensure("DLG_ROOM_TOY", L("Melantha", "Ele ignorava quando eu olhava."), L("Melantha", "Esperava que eu saísse para brincar."));
            Ensure("DLG_ROOM_PHOTO_01", L("Melantha", "Uma imagem preserva contorno."), L("Melantha", "É fácil confundir contorno com presença."));
            Ensure("DLG_ROOM_PHOTO_02", L("Melantha", "Não foi para isso que desenhei o selo."), L("Melantha", "Foi?"));
            Ensure("DLG_ROOM_WINDOW_01", L("Melantha", "Cheia outra vez."), L("Melantha", "Não. Isso não está certo."));
            Ensure("DLG_ROOM_WINDOW_02", L("Melantha", "Claro."), L("Melantha", "A cortina era o problema."));
            Ensure("DLG_ROOM_CANDLES", L("Melantha", "Não."), L("Melantha", "Você não recebe mais intenção esta noite."));
            Ensure("DLG_ROOM_RITUAL_ITEM", L("Melantha", "Encerrado."), L("Melantha", "Eu disse: encerrado."));
            Ensure("DLG_ROOM_GRIMOIRE_LOCKED", L("Melantha", "Não preciso ler de novo."), L("Melantha", "Precisar e querer raramente são a mesma coisa."));
            Ensure("DLG_ROOM_GRIMOIRE_REVEAL", L("Narração", "MEMÓRIA NÃO É ALMA.\nFORMA NÃO É PRESENÇA.\nUM NOME ABRE.\nUM VÍNCULO SUSTENTA.\nUM PREÇO FECHA."), L("Melantha", "Um preço fecha."), L("Melantha", "Eu não escrevi essa linha."));
            Ensure("DLG_ROOM_BED_LOCKED_01", L("Melantha", "Ainda não. Se eu dormir agora, vou levar o dia inteiro comigo."));
            Ensure("DLG_ROOM_BED_LOCKED_02", L("Melantha", "O corpo está cansado. A cabeça ainda não entendeu."));
            Ensure("DLG_ROOM_SLEEP_CONFIRM", L("Melantha", "Só preciso dormir."), L("Melantha", "É uma frase simples."));
            Ensure("DLG_DREAM_TRANSITION", L("Melantha", "Não vou chamar por você."), L("Melantha", "Não esta noite."));
            Ensure("DLG_LABYRINTH_WAKE", L("Melantha", "Estou acordada."), L("Melantha", "Essa não foi uma pergunta."), L("Melantha", "O reflexo lembra do quarto."), L("Melantha", "Não parece lembrar de mim."));
            Ensure("DLG_POE_REVEAL", L("Melantha", "Poe?"), L("Melantha", "Não."), L("Melantha", "Não vou decidir o que você é só porque reconheço a forma."), L("Melantha", "Mas também não vou deixar você desaparecer de novo."));
            Ensure("DLG_ANDREALPHUS_01", L("Andrealphus", "Ela finalmente trouxe o silêncio consigo."), L("Melantha", "Quem é você?"), L("Andrealphus", "Uma pergunta muito sólida para este lugar."), L("Melantha", "Você sabe meu nome."), L("Andrealphus", "Você também sabe o dele."), L("Andrealphus", "E ainda assim hesitou."), L("Melantha", "Aquilo pode não ser Poe."), L("Andrealphus", "Pode? Que palavra misericordiosa."), L("Melantha", "Onde estou?"), L("Andrealphus", "Entre uma intenção e a coragem de admitir qual era."), L("Melantha", "Isso não responde."), L("Andrealphus", "Respostas são portas muito confiantes. Aqui, as confiantes costumam levar ao mesmo lugar."), L("Melantha", "Quero sair."), L("Andrealphus", "Naturalmente. Todos chamam de saída a direção em que ainda não sofreram."), L("Melantha", "E você pretende impedir?"), L("Andrealphus", "Não. Pretendo observar qual de vocês duas chega primeiro."), L("Melantha", "Duas?"));
            Ensure("DLG_ECHO_CORRIDOR_HINT", L("Melantha", "O caminho não está escondido. Está repetindo a pergunta."));
            Ensure("DLG_EMPTY_CHAMBER", L("Melantha", "Eu guardei isso."), L("Andrealphus", "Guardar é apenas esconder algo de uma versão futura de si mesma."));
            Ensure("DLG_VOICE_WELL", L("Voz de Melantha", "Não vou chamar por você."), L("Voz", "Eu não parei de chamar."), L("Melantha", "O Labirinto não repete."), L("Melantha", "Ele corrige."));
            Ensure("DLG_GARDEN_CRESCENT", L("Melantha", "Ela precisava que eu deixasse de querer."));
            Ensure("DLG_GARDEN_FULL", L("Melantha", "O reflexo não está copiando o jardim."), L("Melantha", "O jardim está copiando o reflexo."));
            Ensure("DLG_GARDEN_WANING", L("Melantha", "Subtrair também pode completar."));
            Ensure("DLG_MIRROR_PRESENT", L("Melantha", "Eu esperava desconfiar do reflexo."), L("Melantha", "Não da ausência."));
            Ensure("DLG_MIRROR_DELAYED", L("Narração", "O reflexo repete os movimentos de Melantha com atraso de três segundos."), L("Melantha", "Não gosto quando uma superfície tem expectativas."));
            Ensure("DLG_MIRROR_AHEAD", L("Melantha", "Não gosto quando uma superfície tem expectativas."));
            Ensure("DLG_MIRROR_NO_POE", L("Narração", "Melantha aparece no espelho, mas Poe não."), L("Narração", "Algo arranha o vidro por dentro."));
            Ensure("DLG_MIRROR_TWO_POES", L("Narração", "Um Poe acompanha Melantha. Outro permanece parado, olhando para o jogador."));
            Ensure("DLG_MIRROR_ROOM", L("Melantha", "Se isso é agora, alguém saiu."), L("Melantha", "Se é antes, alguém entrou."));
            Ensure("DLG_ANDREALPHUS_02", L("Andrealphus", "Você escolheu muito bem."), L("Melantha", "Não confio em elogios vindos de um espelho."), L("Andrealphus", "Nem deveria. Eles costumam admirar apenas o que conseguem inverter."), L("Melantha", "Você sabia que eu viria."), L("Andrealphus", "Saber é uma palavra que as pessoas usam quando esquecem quantas vezes tentaram."), L("Melantha", "Já estive aqui?"), L("Andrealphus", "Você trouxe partes de si. Algumas chegaram antes."), L("Melantha", "O que este lugar quer?"), L("Andrealphus", "Lugares não querem. Mas intenções, quando abandonadas, tornam-se péssimas em aceitar o fim."), L("Melantha", "O ritual."), L("Andrealphus", "Qual deles?"), L("Melantha", "Poe está morto."), L("Andrealphus", "Essa é a frase mais verdadeira que você disse. Não é a única verdade disponível."), L("Melantha", "Ele está aqui?"), L("Andrealphus", "Você ainda pergunta como se ‘aqui’ fosse um lugar."), L("Andrealphus", "Entre, Melantha. A Lua prefere pessoas que confundem reconhecimento com certeza."));
            Ensure("DLG_FALSE_FULL_MOON_DOOR", L("Melantha", "Claro. Fácil demais."), L("Andrealphus", "A certeza é muito eficiente em criar círculos."));
            Ensure("DLG_SIGIL_HINT_01", L("Melantha", "As fases estão certas."));
            Ensure("DLG_SIGIL_HINT_02", L("Melantha", "Os objetos não estão em ordem de uso."));
            Ensure("DLG_SIGIL_HINT_03", L("Melantha", "O selo não está incompleto."), L("Melantha", "Está recusando uma conclusão."));
            Ensure("DLG_MOON_DOMAIN_GATE", L("Melantha", "Você está me levando até lá."), L("Melantha", "Ou eu estou seguindo porque preciso acreditar que reconheço você."), L("Andrealphus", "A primeira porta sempre parece chegada."), L("Melantha", "E não é?"), L("Andrealphus", "Você ainda está pensando em lugares."), L("Andrealphus", "Bem-vinda à negação."));
            Ensure("DLG_ROOM_GRIMOIRE_REVEAL", L("Narração", "MEMÓRIA NÃO É ALMA."), L("Narração", "FORMA NÃO É PRESENÇA."), L("Narração", "UM NOME ABRE."), L("Narração", "UM VÍNCULO SUSTENTA."), L("Narração", "UM PREÇO FECHA."), L("Melantha", "Um preço fecha."), L("Melantha", "Eu não escrevi essa linha."));
            Ensure("DLG_ANDREALPHUS_01", L("Andrealphus", "Ela finalmente trouxe o silêncio consigo."), L("Melantha", "Quem é você?"), L("Andrealphus", "Uma pergunta muito sólida para este lugar."), L("Melantha", "Você sabe meu nome."), L("Andrealphus", "Você também sabe o dele."), L("Andrealphus", "E ainda assim hesitou."), L("Melantha", "Aquilo pode não ser Poe."), L("Andrealphus", "Pode?"), L("Andrealphus", "Que palavra misericordiosa."), L("Melantha", "Onde estou?"), L("Andrealphus", "Entre uma intenção e a coragem de admitir qual era."), L("Melantha", "Isso não responde."), L("Andrealphus", "Respostas são portas muito confiantes."), L("Andrealphus", "Aqui, as confiantes costumam levar ao mesmo lugar."), L("Melantha", "Quero sair."), L("Andrealphus", "Naturalmente."), L("Andrealphus", "Todos chamam de saída a direção em que ainda não sofreram."), L("Melantha", "E você pretende impedir?"), L("Andrealphus", "Não."), L("Andrealphus", "Pretendo observar qual de vocês duas chega primeiro."), L("Melantha", "Duas?"));
            Ensure("DLG_ECHO_CORRIDOR_HINT", L("Melantha", "O jogador deve seguir a voz que altera a memória."));
            Ensure("DLG_MIRROR_DELAYED", L("Narração", "Repete os movimentos do jogador com atraso de três segundos."), L("Melantha", "Não gosto quando uma superfície tem expectativas."));
            Ensure("DLG_MIRROR_NO_POE", L("Narração", "Melantha aparece; Poe não."), L("Narração", "Depois de alguns segundos, algo arranha o vidro por dentro."));
            Ensure("DLG_MIRROR_TWO_POES", L("Narração", "Um Poe acompanha Melantha. Outro permanece parado olhando para o jogador."));
            Ensure("DLG_ANDREALPHUS_02", L("Andrealphus", "Você escolheu muito bem."), L("Melantha", "Não confio em elogios vindos de um espelho."), L("Andrealphus", "Nem deveria."), L("Andrealphus", "Eles costumam admirar apenas o que conseguem inverter."), L("Melantha", "Você sabia que eu viria."), L("Andrealphus", "Saber é uma palavra que as pessoas usam quando esquecem quantas vezes tentaram."), L("Melantha", "Já estive aqui?"), L("Andrealphus", "Você trouxe partes de si."), L("Andrealphus", "Algumas chegaram antes."), L("Melantha", "O que este lugar quer?"), L("Andrealphus", "Lugares não querem."), L("Andrealphus", "Mas intenções, quando abandonadas, tornam-se péssimas em aceitar o fim."), L("Melantha", "O ritual."), L("Andrealphus", "Qual deles?"), L("Melantha", "Pare de responder com perguntas."), L("Andrealphus", "Pare de fazer perguntas cujas respostas já desenhou no chão."), L("Melantha", "Poe está morto."), L("Andrealphus", "Essa é a frase mais verdadeira que você disse."), L("Andrealphus", "Não é a única verdade disponível."), L("Melantha", "Ele está aqui?"), L("Andrealphus", "Você ainda pergunta como se ‘aqui’ fosse um lugar."), L("Andrealphus", "Entre, Melantha."), L("Andrealphus", "A Lua prefere pessoas que confundem reconhecimento com certeza."));
            // Literal bindings retained from the approved narrative source.
            Ensure("DLG_ROOM_GRIMOIRE_REVEAL", L("Narração", "MEMÓRIA NÃO É ALMA."), L("Narração", "FORMA NÃO É PRESENÇA."), L("Narração", "UM NOME ABRE."), L("Narração", "UM VÍNCULO SUSTENTA."), L("Narração", "UM PREÇO FECHA."), L("Narração", "Um nome abre. Um vínculo sustenta. Um preço fecha."), L("Melantha", "Um preço fecha."), L("Melantha", "Eu não escrevi essa linha."));
            Ensure("DLG_ECHO_CORRIDOR_HINT", L("Narração", "O que cresce sem nascer, desaparece sem morrer e retorna sem lembrar?"), L("Melantha", "Uma pergunta antes do vocabulário."), L("Melantha", "O jogador deve seguir a voz que altera a memória."));
            Ensure("DLG_EMPTY_CHAMBER", L("Narração", "Você encontrou o que trouxe."), L("Melantha", "Eu guardei isso."), L("Andrealphus", "Guardar é apenas esconder algo de uma versão futura de si mesma."));
            Ensure("DLG_GARDEN_CRESCENT", L("Narração", "Aquilo que se persegue aprende a fugir."), L("Melantha", "Ela precisava que eu deixasse de querer."));
            Ensure("DLG_GARDEN_FULL", L("Melantha", "O reflexo não está copiando o jardim."), L("Melantha", "O jardim está copiando o reflexo."), L("Narração", "A Lua não abre. Ela permite ser lembrada."), L("Melantha", "Isso pareceu uma distinção importante."));
            Ensure("DLG_GARDEN_WANING", L("Narração", "Nem toda perda é falha."), L("Melantha", "Subtrair também pode completar."));
            Ensure("DLG_FALSE_FULL_MOON_DOOR", L("Melantha", "Claro. Fácil demais."), L("Andrealphus", "A certeza é muito eficiente em criar círculos."), L("Narração", "Antes da primeira fase, a Lua já estava."));
            Ensure("DLG_C4C_D01_MOON_ARRIVAL", L("Melantha", "A Lua não parece acima de mim."), L("Melantha", "Parece uma coisa que o Labirinto deixou para trás."), L("Poe", "Não desapareça."));
            Ensure("DLG_C4C_D04_CRESCENT_PROGRESS", L("Melantha", "Ele não abriu a flor. Abriu espaço para ela."));
            Ensure("DLG_C4C_D06_FULL_HINT", L("Melantha", "A pétala não procura o jardim. Procura o lugar que o jardim repete."));
            Ensure("DLG_C4C_D07_FULL_PROGRESS", L("Melantha", "O reflexo aceitou a posição antes de mim."));
            Ensure("DLG_C4C_D08_WANING_HINT", L("Melantha", "O que a Lua retira também obedece a uma ordem."));
            Ensure("DLG_C4C_D10_GARDEN_COMPLETE", L("Poe", "Agora."), L("Melantha", "O jardim ficou quieto. Não vazio."));
            Ensure("DLG_C4C_D11_MIRROR_INTRO", L("Melantha", "Cada espelho mostra uma versão da mesma pergunta."));
            Ensure("DLG_C4C_D12_MIRROR_ERROR", L("Melantha", "Esse reflexo não me reconhece."));
            Ensure("DLG_C4C_D13_MIRROR_PROGRESS", L("Melantha", "Um deles parou de mentir."), L("Poe", "Ainda não."));
            Ensure("DLG_C4C_D15_CORRIDOR_REPEAT", L("Melantha", "O marco voltou antes de eu voltar."));
            Ensure("DLG_C4C_D17_CORRIDOR_HINT", L("Melantha", "Não é a distância que muda. É aquilo que a memória aceita."));
            Ensure("DLG_C4C_D18_CORRIDOR_SUCCESS", L("Melantha", "Desta vez, o retorno não me trouxe de volta."), L("Poe", "Siga."));
            Ensure("DLG_C4C_D20_SIGIL_ERROR", L("Melantha", "O anel recusou a intenção, não o movimento."));
            Ensure("DLG_C4C_D21_SIGIL_PROGRESS", L("Melantha", "Uma parte respondeu. As outras ainda estão ouvindo."));
            Ensure("DLG_C4C_D22_SIGIL_SUCCESS", L("Poe", "Ali."), L("Melantha", "O fragmento estava esperando a conclusão."));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static DialogueLine L(string speaker, string text) => new DialogueLine { Speaker = speaker, Text = text, CharactersPerSecond = 38f, LockMovement = true };
        static void Ensure(string id, params DialogueLine[] lines)
        {
            var path = Root + "/" + id + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<DialogueData>(); AssetDatabase.CreateAsset(asset, path); }
            asset.Configure(id, lines);
            EditorUtility.SetDirty(asset);
        }
    }
}
