const { onRequest } = require("firebase-functions/v2/https");
const admin = require("firebase-admin");
admin.initializeApp();

const db = admin.database(); // Adjusted to use Realtime Database

const GAME_KEY = "gameData";
const ROOMS_KEY = "rooms";
const ROOM_STATUS_KEY = "Status";

exports.createRoom = onRequest((req, res) => {
    
    const roomId = req.body.Id;
    const jsonData = JSON.parse(req.body.JsonData);
    
    console.log(jsonData);
    
    const roomRef = db.ref(`${GAME_KEY}/${ROOMS_KEY}/${roomId}`);
    roomRef.once('value', snapshot => {
        if (snapshot.exists()) {
            res.json({ success: false, message: "Room with that ID already exists." });
        } else {
            roomRef.set(jsonData)
                .then(() => res.json({ success: true, message: "Room created successfully" }))
                .catch(error => {
                    console.error("Error creating room: ", error);
                    res.status(500).json({ success: false, message: "Failed to create room" });
                });
        }
    });
});

exports.joinRoom = onRequest((req, res) => {
    const roomType = parseInt(req.body.Type); // Ensure roomType is an integer
    const roomName = req.body.Name; // Room name for matching in friendly mode
    const roomsRef = db.ref(`${GAME_KEY}/${ROOMS_KEY}`).orderByChild(ROOM_STATUS_KEY).equalTo(0);

    const playerData = JSON.parse(req.body.PlayerData);

    roomsRef.once('value', snapshot => {
        if (snapshot.exists() && snapshot.hasChildren()) {
            const rooms = snapshot.val();
            let foundRoom = false;

            for (const roomId in rooms) {
                if (rooms.hasOwnProperty(roomId)) {
                    const room = rooms[roomId];
                    // Check for room availability and type
                    if (room.Status === 0 && room.Type === roomType) {
                        // Additional name check for friendly type
                        if (roomType === 0 && room.Name !== roomName) {
                            continue; // Skip to the next room if names do not match for friendly type
                        }
                        foundRoom = true;
                        const roomRef = db.ref(`${GAME_KEY}/${ROOMS_KEY}/${roomId}`);

                        const updatedRoomPlayers = room.RoomPlayers ? [...room.RoomPlayers, playerData] : [playerData];

                        roomRef.update({
                            RoomPlayers: updatedRoomPlayers,
                            Status: 1 // Change status to occupied or not available
                        })
                            .then(() => {
                                res.status(200).send({ success: true, room: { ...room, RoomPlayers: updatedRoomPlayers, Id: roomId } });
                            })
                            .catch(error => {
                                console.error("Error updating room with new player: ", error);
                                res.status(500).send({ success: false, message: "Failed to join room." });
                            });

                        break; // Exit the loop once a matching room is found and updated
                    }
                }
            }

            // If no room found, handle based on roomType. For friendly type, specify no room with the name found.
            if (!foundRoom) {
                if (roomType === 0) {
                    res.status(200).send({ success: false, message: `No available rooms found with the name "${roomName}" in Friendly mode.` });
                } else {
                    res.status(200).send({ success: false, message: "No available rooms found." });
                }
            }
        } else {
            res.status(200).send({ success: false, message: "No available rooms found." });
        }
    }, error => {
        console.error("Error fetching rooms: ", error);
        res.status(500).send({ success: false, message: "Failed to fetch rooms." });
    });
});

exports.leaveRoom = onRequest((req, res) => {
    const { roomId, playerId } = req.body;

    const roomRef = db.ref(`${GAME_KEY}/${ROOMS_KEY}/${roomId}`);

    roomRef.once('value', snapshot => {
        if (!snapshot.exists()) {
            return res.status(200).json({ success: false, message: "Room not found." });
        }

        const room = snapshot.val();
        const updatedRoomPlayers = room.RoomPlayers.filter(player => player.Id !== playerId);

        if (updatedRoomPlayers.length === 0 && room.Status === 0) {
            // If no players are left in the room and room status is 0, delete the room
            roomRef.remove()
                .then(() => res.json({ success: true, message: "Room deleted successfully" }))
                .catch(error => {
                    console.error("Error deleting room: ", error);
                    res.status(500).json({ success: false, message: "Failed to delete room" });
                });
        } else {
            // Otherwise, just update the room's players list
            roomRef.update({ RoomPlayers: updatedRoomPlayers })
                .then(() => res.json({ success: true, message: "Left room successfully", roomId: roomId }))
                .catch(error => {
                    console.error("Error leaving room: ", error);
                    res.status(500).json({ success: false, message: "Failed to leave room" });
                });
        }
    });
});