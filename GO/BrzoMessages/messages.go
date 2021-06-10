package main

// MessageData struct
type MessageData struct {
	Version int `json:"version"`
	Body    struct {
		Data string `json:"Data"`
	} `json:"body"`
}

// MessageReceived struct
type MessageReceived struct {
	Type string `json:"type"`
	Data struct {
		Text string `json:"text"`
		Info struct {
			ID        string `json:"Id"`
			RemoteJid string `json:"RemoteJid"`
			SenderJid string `json:"SenderJid"`
			FromMe    bool   `json:"FromMe "`
			Timestamp int    `json:"Timestamp"`
			PushName  string `json:"PushName"`
			Status    int    `json:"Status"`
		} `json:"Info"`
		ContextInfo struct {
			QuotedMessageID string      `json:"QuotedMessageID"`
			QuotedMessage   interface{} `json:"QuotedMessage"`
			Participant     string      `json:"Participant"`
			IsForwarded     bool        `json:"IsForwarded"`
		} `json:"ContextInfo"`
	} `json:"data"`
	File string `json:"file"`
	WID  string `json:"wid"`
}

// MessageAck struct
type MessageAck struct {
	Cmd  string `json:"cmd"`
	ID   string `json:"id"`
	Ack  int    `json:"ack"`
	From string `json:"from"`
	To   string `json:"to"`
	T    int    `json:"t"`
}
