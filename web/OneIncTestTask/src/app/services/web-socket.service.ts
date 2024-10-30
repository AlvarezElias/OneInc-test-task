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
  private openSubject = new Subject<string>();
  private _connectionId: string | null;

  constructor(private http: HttpClient) { 
    this._connectionId = crypto.randomUUID();
  }

  connect(): void {

    if(!this.webSocket || this.webSocket.readyState == WebSocket.CLOSED) {
      
      this._connectionId ??= crypto.randomUUID();

      this.webSocket = new WebSocket(`ws://localhost:5160/api/Encoding/connect?connectionId=${this._connectionId}`);

      this.webSocket.onopen = () => {
        this.isProcessing = true;
        this.openSubject.next("Connected");
      };

      this.webSocket.onmessage = (event) => {
        this.messageSubject.next(event.data);
      };

      this.webSocket.onclose = () =>  {
        this.close();
      };

      this.webSocket.onerror = () => {
        this.close();
      };
    }
  }

  isConnecting(): boolean | null {
    return this.webSocket && this.webSocket.readyState == WebSocket.CONNECTING;
  }

  isOpenConnection(): boolean | null {
    return this.webSocket && this.webSocket.readyState == WebSocket.OPEN;
  }

  sendMessage(message: string): void {
    if(this.webSocket && this.webSocket.readyState == WebSocket.OPEN)
      this.webSocket.send(message);
  }

  get messages$(): Observable<string> {
    return this.messageSubject.asObservable();
  }

  get open$(): Observable<string> {
    return this.openSubject.asObservable();
  }

  cancel(): void {
    if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN)
      this.http.post('http://localhost:5160/api/Encoding/cancel', 
        { connectionId: this._connectionId },
        {
          headers: { 'Content-Type': 'application/json' }
        }
      ).subscribe({
        next: success => {
          console.log('Proceso cancelado');
          this.isProcessing = false;
        }, 
        error: err => {
          console.error('Error cancelando el proceso', err);
        }
      });
  }

  close(): void {
    this.isProcessing = false;
    this.webSocket?.close();
    this._connectionId = crypto.randomUUID();
  }
}
