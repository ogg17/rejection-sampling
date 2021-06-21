function draw() {
    let canvas = document.getElementById('canvas');
    if (canvas.getContext) {
        $.get('/result', {handler: 'graph'}, function (data){   
                       
            let drawRoot = JSON.parse(data);
            let drawParam = drawRoot.drawParam;          
            
            let ctx = canvas.getContext('2d');
            
            // Settings
            ctx.font = "50px sans-serif";
            ctx.lineCup = "round";
            ctx.lineWidth = 5;            
            //

            // Axis 
            let offsetLine = 18;
            let offsetFont = 90;
            ctx.strokeStyle = 'rgb(77, 77, 77)';
            ctx.moveTo(drawParam.Offset, changeY(0));
            ctx.lineTo(drawParam.Width - drawParam.Offset, changeY(0));
            ctx.moveTo(drawParam.Offset, drawParam.Offset);
            ctx.lineTo(drawParam.Offset, drawParam.Height - drawParam.Offset);
            ctx.lineWidth = 4;
            let deltaX = Math.abs(drawParam.xMaximum - drawParam.xMinimum) / 10;
            for(let x = drawParam.xMinimum; x <= drawParam.xMaximum + deltaX/2; x += deltaX){
                ctx.moveTo(changeX(x), changeY(0) - offsetLine);
                ctx.lineTo(changeX(x), changeY(0) + offsetLine);
                ctx.fillText(x.toFixed(1), changeX(x) - offsetLine*2, changeY(0) + offsetFont - offsetLine);
            }
            for(let y = drawParam.yMinimum; y <= drawParam.yMaximum; y+= drawParam.yMaximum / 10){
                ctx.moveTo(drawParam.Offset - offsetLine, changeY(y));
                ctx.lineTo(drawParam.Offset + offsetLine, changeY(y));
                ctx.fillText(y.toFixed(1), drawParam.Offset - offsetFont - offsetLine, changeY(y) + offsetLine);
            }
            
            //

            //Histogram            
            ctx.strokeStyle = 'rgb(77, 77, 77)'
            ctx.fillStyle = '#d9d9d9';
            for(let i = 0; i < drawRoot.drawFuncHistRectsLength; i++){
                ctx.fillRect(drawRoot.drawFuncHistRects[i].X, drawRoot.drawFuncHistRects[i].Y,
                    drawRoot.drawFuncHistRects[i].Width, drawRoot.drawFuncHistRects[i].Height);
                ctx.strokeRect(drawRoot.drawFuncHistRects[i].X, drawRoot.drawFuncHistRects[i].Y,
                    drawRoot.drawFuncHistRects[i].Width, drawRoot.drawFuncHistRects[i].Height);
            }
            
            //
            
            // Function
            ctx.strokeStyle = '#000099';
            ctx.moveTo(drawRoot.drawFuncPoints[0].X, drawRoot.drawFuncPoints[0].Y)
            for(let i = 1; i < drawRoot.drawFuncPointsLength; i++)              
                ctx.lineTo(drawRoot.drawFuncPoints[i].X, drawRoot.drawFuncPoints[i].Y);
            //          
            
            ctx.stroke();

            function changeX(varX)
            {
                return (drawParam.Width - 2 * drawParam.Offset) * (varX - drawParam.xMinimum) / (drawParam.xMaximum - drawParam.xMinimum) + drawParam.Offset;
            }

            function changeY(varY)
            {
                return (drawParam.Height - 2 * drawParam.Offset) * (-varY + drawParam.yMaximum) / (drawParam.yMaximum - drawParam.yMinimum) + drawParam.Offset;
            }
        })        
    }
}