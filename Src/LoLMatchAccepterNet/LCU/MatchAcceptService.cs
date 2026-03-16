namespace LoLMatchAccepterNet.LCU
{
    public class MatchAcceptService
    {
        private readonly Game _game;

        public MatchAcceptService(Game game)
        {
            _game = game;
        }

        public async Task RunAutoAcceptLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    bool isMatchAccepted = await _game.WaitForQueue();

                    if (isMatchAccepted)
                    {
                        string currentPhase = await _game.WaitUntilPhaseEnds(Game.ReadyCheck);

                        if (currentPhase == Game.ChampSelect)
                        {
                            Console.WriteLine("Currently in champ select...");
                            await _game.WaitUntilPhaseEnds(Game.ChampSelect);
                        }
                    }

                    bool inGame = await _game.IsActive();

                    if (inGame)
                    {
                        Console.WriteLine("Active game detected. Waiting until game ends...");
                        await _game.WaitUntilGameEnds();
                        Console.WriteLine("Game ended. Navigating to lobby...");
                        await _game.NavigateToLobby();
                        Console.WriteLine("Resuming auto-accept...");
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error communicating with League client.");
                    await Task.Delay(6000, cancellationToken);
                    throw;
                }

                await Task.Delay(500, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
