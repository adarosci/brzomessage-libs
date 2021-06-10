package main

import (
	"encoding/json"
	"errors"
	"fmt"
	"io/ioutil"
	"net/http"
	"strings"
)

type connection struct {
	token, privateKey string
}

func (a connection) create(url, content string) (*http.Client, *http.Request, error) {
	payload := strings.NewReader(content)

	client := &http.Client{}
	req, err := http.NewRequest("POST", url, payload)
	if err != nil {
		return nil, nil, err
	}
	req.Header.Add("Content-Type", "application/json")
	req.Header.Add("Authorization", fmt.Sprintf("Bearer %v", a.privateKey))
	return client, req, nil
}

func (a connection) autenticate() (key string, err error) {
	url := fmt.Sprintf("%v?token=%v&wait=true", AuthURL, a.token)
	client, req, _ := a.create(url, "")
	res, err := client.Do(req)
	if err != nil {
		return
	}
	if res.StatusCode != http.StatusOK {
		err = errors.New("Falha na autenticação token ou privateKey invalidos")
		return
	}
	defer res.Body.Close()
	body, err := ioutil.ReadAll(res.Body)
	key = string(body)
	return
}

func (a connection) start() (lastMessageID string, err error) {
	url := fmt.Sprintf("%v?token=%v", ConnectURL, a.token)
	client, req, _ := a.create(url, "")
	res, err := client.Do(req)
	if err != nil {
		return
	}
	if res.StatusCode != http.StatusOK {
		lastMessageID = ""
		return
	}
	defer res.Body.Close()
	body, err := ioutil.ReadAll(res.Body)
	lastMessageID = string(body)
	return
}

func (a connection) stop() (err error) {
	url := fmt.Sprintf("%v?token=%v", DisconnectURL, a.token)
	client, req, _ := a.create(url, "")
	res, err := client.Do(req)
	if err != nil {
		return
	}
	if res.StatusCode != http.StatusOK {
		err = errors.New("Não foi possivel desconectar")
		return
	}
	return
}

func (a connection) confirm(id, remoteJid string) (err error) {
	url := fmt.Sprintf("%v?token=%v", ConfirmURL, a.token)

	content, _ := json.Marshal(map[string]string{
		"Id":        id,
		"RemoteJid": remoteJid,
	})
	client, req, _ := a.create(url, string(content))
	res, err := client.Do(req)
	if err != nil {
		return
	}
	if res.StatusCode != http.StatusOK {
		err = errors.New("Não foi possivel confirmar mensagem")
		return
	}
	return
}
