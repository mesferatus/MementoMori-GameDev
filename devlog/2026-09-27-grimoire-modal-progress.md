# Grimório — progresso de 27/09/2026

- O objeto físico do Quarto continua no lugar. A interface passa a usar um controlador persistente criado na primeira interação.
- O acesso global usa `G`: `S` já está associado a movimento e navegação no Input System.
- A abertura exige desbloqueio para o atalho global e respeita diálogo, pausa e `InputGate`.
- Ao abrir, o Grimório suspende os outros Canvas e bloqueia input de gameplay; ao fechar, restaura os estados anteriores.
- A primeira interação aguarda o término de interfaces bloqueantes antes de abrir o livro.
- Verificado no Unity Editor: compilação sem erros; abertura no Quarto; presença do controlador e abertura no Labirinto e no Domínio da Lua; `ResetSession` retira o desbloqueio.
- Em 28/09, o usuário forneceu nesta conversa o texto integral das 40 entradas canônicas. `Assets/Resources/Grimoire/CanonicalEntries.txt` contém A01–A12, R01–R07, P01–P13 e M01–M08. A contagem em execução foi 12/7/13/8 = 40; P12 tem somente os dois níveis canônicos.
- As quatro abas, desbloqueios progressivos, paginação de entradas longas, acesso global por `G`, comportamento modal e persistência entre Quarto, Labirinto e Domínio da Lua foram implementados. A P12 agora é descoberta na ativação da sequência do Espelho Negro.
- A fonte dentro das páginas do livro é preta; os títulos externos das abas permanecem lilás. A nova folha `Assets/Resources/UI/Grimoire/GRIMOIRE_UI_SHEET_V2.png` fornece os estados dos botões e setas.
- O aviso de interação usa tecla `E` menor e texto contido na faixa. Objetivo e aviso foram conferidos em `Evidencias/Screenshots/Objective_Interaction_final.png`.
- O FinalBeta usa a fundação visual do Quarto atual e os mesmos parâmetros de câmera Pixel Perfect. Conferido em `Evidencias/Screenshots/FinalBeta_QuartoAtual_v3.png`.
- Capturas finais das quatro abas, entrada longa e entrada nova: `Evidencias/Screenshots/01_Grimoire_Notes.png` a `06_Grimoire_NewEntry.png`. As entradas extras usadas nas capturas foram desbloqueadas temporariamente apenas em Play Mode.
- Validação final: 25/25 testes EditMode e 13/13 PlayMode aprovados; zero erros de compilação. O Console ainda registra conflito de porta do Unity Pipeline e avisos de scripts ausentes durante testes de cenas, não resolvidos neste escopo.
- Sem commit e sem push.
