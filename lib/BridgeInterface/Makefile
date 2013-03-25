.dummy: package SampleBridgeInterface.tgz

SRC_DIR=BridgeInterface

package: SampleBridgeInterface.tgz

SampleBridgeInterface.tgz:
	tar zcf SampleBridgeInterface.tgz \
		$(SRC_DIR)/BridgeInterface/Message.h \
		$(SRC_DIR)/BridgeInterface/MessageException.h \
		$(SRC_DIR)/BridgeInterface/Session.h \
		$(SRC_DIR)/BridgeInterface/Socket.h \
		$(SRC_DIR)/BridgeInterface/stdafx.h \
		$(SRC_DIR)/BridgeInterface/Thread.h \
		$(SRC_DIR)/BridgeInterface/BridgeInterface.cpp \
		$(SRC_DIR)/BridgeInterface/Message.cpp \
		$(SRC_DIR)/BridgeInterface/Session.cpp \
		$(SRC_DIR)/BridgeInterface/Socket.cpp \
		$(SRC_DIR)/BridgeInterface/stdafx.cpp \
		$(SRC_DIR)/BridgeInterface/Thread.cpp \
		$(SRC_DIR)/BridgeInterface/BridgeInterface.vcproj \
		$(SRC_DIR)/BridgeInterface/notes.txt \
		$(SRC_DIR)/BridgeInterface/ReadMe.txt \
		$(SRC_DIR)/SampleBridgeClient/OA_Messages.h \
		$(SRC_DIR)/SampleBridgeClient/stdafx.h \
		$(SRC_DIR)/SampleBridgeClient/OA_Messages.cpp \
		$(SRC_DIR)/SampleBridgeClient/SampleBridgeClient.cpp \
		$(SRC_DIR)/SampleBridgeClient/stdafx.cpp \
		$(SRC_DIR)/SampleBridgeClient/SampleBridgeClient.vcproj \
		$(SRC_DIR)/SampleBridgeClient/ReadMe.txt \
		$(SRC_DIR)/SampleBridgeInterface.sln
