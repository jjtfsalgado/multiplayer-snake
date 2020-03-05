import * as signalR from "@microsoft/signalr";
import {HubConnection} from "@microsoft/signalr";
import {canvas, ctx, Snake} from "./snake";

export class Game {
    apple;
    snakes: Array<Snake> = [];
    snake: Snake;
    connection: HubConnection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder().withUrl("http://192.168.5.45:5000/snakeHub").build();
        this.connection.on("Snakes", this.onSnakes);
        window.addEventListener("close", this.onWindowClose);
    }

    private onWindowClose = async () => {
        // await this.connection.invoke("disconnectedSnake", JSON.stringify(this.snake));

        await this.connection.stop();
    };

    start = async () => {
        this.renderFrame();

        try{
            await this.connection.start();
            this.snake = new Snake({id: this.connection.connectionId, description: "Jose"});
            this.snakes.push(this.snake);

            await this.connection.invoke("connectedSnake", JSON.stringify(this.snake));
            console.info("connected");
        } catch (e) {
            console.error(e.toString());
        }
    };

    onSnakes = (snakes) => {
        const s: Array<any> = JSON.parse(snakes);
        console.log("hey i just received some snakes -> ", s);

        this.snakes = s.map(i => {
            const existing = this.snakes.find(j => j.id === i.id);

            if (existing) {
                Object.assign(existing, i);

                return existing;
            } else {
                return new Snake(i);
            }
        });
    };

    renderFrame = () => {
        ctx.clearRect(0 ,0 , canvas.width, canvas.height);
        this.snakes.forEach(i => i.render(i.id === this.connection.connectionId));
        this.connection.invoke("onMove", JSON.stringify(this.snake));

        requestAnimationFrame(this.renderFrame)
    };
}

// const sn1 = new Snake( "Jose");
const game = new Game();

(async () => {
    await game.start()
})();
