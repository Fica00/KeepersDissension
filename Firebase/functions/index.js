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
    const roomsRef = db.ref(`${GAME_KEY}/${ROOMS_KEY}`).orderByChild(ROOM_STATUS_KEY).equalTo(0);
    const playerData = JSON.parse(req.body.PlayerData);

    roomsRef.once('value', snapshot => {
        if (snapshot.exists() && snapshot.hasChildren()) {
            const rooms = snapshot.val();
            let foundRoom = false;

            // Loop through the rooms to find an available one
            for (const roomId in rooms) {
                if (rooms.hasOwnProperty(roomId)) {
                    const room = rooms[roomId];
                    if (room.Status === 0) { // Assuming 0 is the code for 'available'
                        foundRoom = true;
                        const roomRef = db.ref(`${GAME_KEY}/${ROOMS_KEY}/${roomId}`);

                        // Assuming RoomPlayers is an array. If it does not exist, initialize as an empty array.
                        const updatedRoomPlayers = room.RoomPlayers ? [...room.RoomPlayers, playerData] : [playerData];

                        // Update the room with the new player and change its status
                        roomRef.update({
                            RoomPlayers: updatedRoomPlayers,
                            Status: 1 // Assuming 1 is the code for 'occupied' or 'not available'
                        })
                            .then(() => {
                                res.status(200).send({ success: true, room: { ...room, RoomPlayers: updatedRoomPlayers, Id: roomId } });
                            })
                            .catch(error => {
                                console.error("Error updating room with new player: ", error);
                                res.status(500).send({ success: false, message: "Failed to join room." });
                            });

                        break; // Exit the loop once the first available room is found and updated
                    }
                }
            }

            if (!foundRoom) {
                res.status(200).send({ success: false, message: "No available rooms found." });
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