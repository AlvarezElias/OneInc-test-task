import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebSocketService {
  public isProcessing = false;
  
  private webSocket: WebSocket | null = null;
  private messageSubject = new Subject<string>();
  private connectionId: string = crypto.randomUUID();

  constructor(private http: HttpClient) { }

  connect(inputText: string): void {

    if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN)
      this.close();
    

    if(!this.webSocket || this.webSocket.readyState == WebSocket.CLOSED){
      this.webSocket = new WebSocket(`ws://localhost:5160/api/Encoding/connect?inputText=${inputText}&connectionId=${this.connectionId}`);

      this.webSocket.onopen = () => {
        this.isProcessing = true;
      };

      this.webSocket.onmessage = (event) => {
        this.messageSubject.next(event.data);
      };

      this.webSocket.onclose = () =>  {
        this.isProcessing = false;
        this.webSocket = null;
      };

      this.webSocket.onerror = () => {
        this.close();
      };
    }
  }

  sendMessage(message: string): void {
    if(this.webSocket && this.webSocket.readyState == WebSocket.OPEN)
      this.webSocket.send(message);
    
  }

  get messages$(): Observable<string> {
    return this.messageSubject.asObservable();
  }

  close(): void {
    if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN){
      this.http.put<string>('http://localhost:5160/api/Encoding/close', 
        { 'connectionId': this.connectionId }, 
        {
          headers: { 
            'Accept': '*/*', 
            'Content-Type': 'text/json',
          }
        }).subscribe({
          next: success => {
            console.log('close connection');
            this.webSocket?.close();
          }, 
          error: err => {
            console.error('error closing connection', err);
          },
          complete: () => console.log("completed")
        });
    }
      
  }
}
