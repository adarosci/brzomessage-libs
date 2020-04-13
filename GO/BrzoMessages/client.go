package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"runtime/debug"
	"time"

	"github.com/go-chi/chi"
	"github.com/go-chi/chi/middleware"
	"github.com/go-chi/cors"
	"github.com/gorilla/websocket"
)

// BrzoMessagesClient client
type BrzoMessagesClient struct {
	accessKey, privateKey string
}

// NewConnect create new connection client messages
func NewConnect(accessKey, privateKey string,
	handlerMessage func(MessageReceived) bool,
	handlerAck func(MessageAck) bool) error {

	conn := connection{accessKey, privateKey}
	auth, err := conn.autenticate()
	if err != nil {
		fmt.Fprintln(os.Stderr, err, string(debug.Stack()))
		<-time.After(time.Millisecond * 250)
		go NewConnect(accessKey, privateKey, handlerMessage, handlerAck)
		return err
	}

	go func() {
		interrupt := make(chan os.Signal, 1)
		signal.Notify(interrupt, os.Interrupt)

		closeConnection := make(chan bool)

		uri := fmt.Sprintf("%v/%v?token=%v&p=%v", SocketURL, accessKey, accessKey, auth)
		c, _, err := websocket.DefaultDialer.Dial(uri, nil)

		if err != nil {
			log.Fatal("dial:", err)
		}
		defer c.Close()

		go func() {
			for {
				_, message, err := c.ReadMessage()
				if err != nil {
					log.Println("read:", err)
					closeConnection <- true
					return
				}

				messageData := MessageData{}
				if er := json.Unmarshal(message, &messageData); er != nil {
					continue
				}

				messageReceived := MessageReceived{}
				if er := json.Unmarshal([]byte(messageData.Body.Data), &messageReceived); er == nil && messageReceived.Type != "" {
					if handlerMessage(messageReceived) {
						conn.confirm(messageReceived.Data.Info.ID, messageReceived.Data.Info.RemoteJid)
					}
					continue
				}

				messageAck := MessageAck{}
				if er := json.Unmarshal([]byte(messageData.Body.Data), &messageAck); er == nil {
					if handlerAck(messageAck) {
						conn.confirm(messageAck.ID, messageAck.To)
					}
					continue
				}
			}
		}()

		go func() {
			// ping
			for {
				<-time.After(time.Second * 20)
				data, _ := json.Marshal(map[string]interface{}{
					"ping": true,
				})
				err := c.WriteMessage(websocket.TextMessage, data)
				if err != nil {
					log.Println("read:", err)
					closeConnection <- true
					return
				}
			}
		}()

		conn.start()

		select {
		case <-closeConnection:
		case <-interrupt:
		}

		conn.stop()

		<-time.After(time.Millisecond * 250)

		go NewConnect(accessKey, privateKey, handlerMessage, handlerAck)
	}()

	return nil
}

func main() {
	r := chi.NewRouter()

	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)
	r.Use(middleware.Logger)
	r.Use(middleware.Recoverer)

	cors := cors.New(cors.Options{
		AllowedOrigins:   []string{"*"},
		AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowedHeaders:   []string{"Accept", "Authorization", "Content-Type", "X-CSRF-Token"},
		ExposedHeaders:   []string{"Link"},
		AllowCredentials: true,
		MaxAge:           300, // Maximum value not ignored by any of major browsers
	})
	r.Use(cors.Handler)

	r.Use(middleware.Timeout(60 * time.Second))

	go NewConnect("cad587f6-4f06-4c9f-9575-ae500b5f161c", "DOvJHQ-CSB-tBs-u2HhE6RhwT2t6nZZ7", messages, ack)

	http.ListenAndServe(":3335", r)
}

func messages(msg MessageReceived) bool {
	fmt.Println(msg)
	return true
}

func ack(msg MessageAck) bool {
	fmt.Println(msg)
	return true
}
