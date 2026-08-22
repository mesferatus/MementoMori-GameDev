# Análise estatística de QA — C5 Final

## Amostra observada

| Gate | Passou | Total | Taxa |
|---|---:|---:|---:|
| EditMode | 24 | 24 | 100,0% |
| PlayMode | 11 | 11 | 100,0% |
| Smoke CT | 11 | 11 | 100,0% |
| **Total** | **46** | **46** | **100,0%** |

## Interpretação

- Falhas observadas: 0.
- Missing References nos logs finais: 0 ocorrências acionáveis.
- Build Windows: gerada com sucesso; tamanho registrado de 183.073.583 bytes.
- Limite inferior do intervalo de confiança binomial de Wilson a 95% para 46/46: **92,3%**.
- A taxa de 100% descreve esta amostra de testes automatizados; não representa probabilidade universal de ausência de defeitos.
- O playthrough humano foi confirmado pelo usuário e é reportado separadamente, sem ser misturado à amostra automatizada.

## Evidências primárias

- `TestResults/master-final-editmode.xml`
- `TestResults/master-final-playmode.xml`
- `TestResults/ct-evidence-current.json`
- `Logs/master-final-editmode-r2.log`
- `Logs/master-final-playmode.log`
- `Logs/master-final-smoke.log`
- `Logs/master-final-build.log`

## Limitações

Esta análise não prova ausência absoluta de código morto nem substitui a revisão visual da Luiza. Ela consolida os gates técnicos executados e os resultados registrados nesta versão.
