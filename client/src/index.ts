const canvas = document.getElementById("canvas") as HTMLCanvasElement;
canvas.height = 500; canvas.width = 600;
const ctx = canvas.getContext('2d');
interface ISnake {
    id: string;
    x: number;
    y: number;
    description: string;
}

class Snake implements ISnake{
    id;
    x;
    y;
    description;

    direction: "xx" | "yy" | "-yy" | "-xx";
    speed = 1;

    constructor(id: string, x: number, y: number, description: string) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.description = description;

        document.addEventListener("keydown", this.onKeyDown)
    }

    private onKeyDown = (ev) => {
        switch (ev.keyCode) {
            case 37:{
                //left arrow
                this.setDirection("-xx");
                break
            }
            case 38:{
                //up arrow
                this.setDirection("yy");
                break
            }
            case 39:{
                //right arrow
                this.setDirection("xx");
                break;
            }
            case 40:{
                //down arrow
                this.setDirection("-yy");
                break;
            }
        }
    };

    start(){
        this.setDirection("xx");
        this.draw()
    }

    setDirection(type: "xx" | "yy" | "-yy" | "-xx"){
        if(this.direction !== type){
            this.direction = type;
        }
    }

    draw = (timestamp?) => {
        let {x,y} = this;

        ctx.clearRect(0 ,0 , canvas.width, canvas.height);
        ctx.beginPath();
        ctx.rect(x, y, 5, 5);
        ctx.fillStyle = 'green';
        ctx.fill();

        switch (this.direction) {
            case "xx": {
                this.x += this.speed;
                break;
            }
            case "-xx": {
                this.x -= this.speed;
                break;
            }
            case "yy": {
                this.y -= this.speed;
                break;
            }
            case "-yy": {
                this.y += this.speed;
                break;
            }
        }

        requestAnimationFrame(this.draw);
    };
}

const sn1 = new Snake("test", canvas.width/2, canvas.height/2, "Jose");
sn1.start();

