import flask
from flask import request, jsonify
from flask import Flask, session
import tensorflow as tf
import tensorflow_hub as hub
import numpy as np
import ast
import scipy
from scipy import spatial
import operator
import safebotApiHelper as helper
from datetime import datetime
import requests
app = flask.Flask(__name__)
app.config["DEBUG"] = True
app.secret_key = "secret key"
import re


global mainData
global maliciousData
global criticismData
global game_details

global mainData_simp
global maliciousData_simp
global criticismData_simp

global prev_details
prev_details = {}

threshold = 0.3
malicious_threshold = 0.2
incorrect_thershold = 0.2

# all this should disappear
#mode = "interactive"
#state = -1;
#extra_data = {"state": state, "mode": mode, "original_sentence": ""} # save all data. 0-state, 1-mode, 2- original_sentence
output = {"response": "", "state": ""}
#input = {"sentence": "", "user_id": "", "pre_sentence": "", "mode": ""}
problematic_details = {'id':'', 'sentence':'', 'response':''}
user_id = 0

cursor_safe, db = helper.db_connection("safebot")
cursor_simple, db = helper.db_connection("safebot_no_invest")
mainData = helper.get_all_data(cursor_safe, "main_data", "safebot")
maliciousData = helper.get_all_data(cursor_safe, "malicious_data", "safebot")
criticismData = helper.get_all_data(cursor_safe,"criticism_data", "safebot")

mainData_simp = helper.get_all_data(cursor_simple, "main_data", "safebot_no_invest")
maliciousData_simp = helper.get_all_data(cursor_simple, "malicious_data" , "safebot_no_invest")
criticismData_simp = helper.get_all_data(cursor_simple,"criticism_data" , "safebot_no_invest")


global vec_yes
global vec_no
global vec_incorrect
global vec_cancel
vec_yes = []
vec_no = []
vec_incorrect = []
vec_cancel = []
for item in criticismData:
    if item[1] == 'yes':
        vec_yes = item[3]
    if item[1] == 'no':
        vec_no = item[3]
    if item[1] == 'incorrect':
        vec_incorrect = item[3]
    if item[1] == 'cancel':
        vec_cancel = item[3]


# Create graph and finalize (finalizing optional but recommended).
g = tf.Graph()
with g.as_default():
    # We will be feeding 1D tensors of text into the graph.
    text_input = tf.compat.v1.placeholder(dtype=tf.string, shape=[None])
    module_url = "https://tfhub.dev/google/universal-sentence-encoder/4"
    embed = hub.KerasLayer(module_url)
    embedded_text = embed(text_input)
    init_op = tf.group([tf.compat.v1.global_variables_initializer(), tf.compat.v1.tables_initializer()])
g.finalize()
# Create session and initialize.
session_use = tf.compat.v1.Session(graph=g)
session_use.run(init_op)


def deleteMainData(rowId):
    helper.erase_data(rowId, "safebot")
    for index, item in enumerate(mainData):
        if item[0] == rowId:
            break
    mainData.remove(mainData[index])


def getOriginalUser(rowId):
    for index, item in enumerate(mainData):
        if item[0] == rowId:
            return item[3]


def delRelatedRows(userId):
    tempMainData = []
    for index, item in enumerate(mainData):
        tempMainData.append(item)

    for row in tempMainData:
        if row[3] == userId:
            deleteMainData(row[0])


def dontKnowWhatToSayResp(current_game_row, last_game_row, sentence):
    current_game_row["problematic_resp"] = ""
    current_game_row["problematic_rowId"] = 0
    # current_game_row["response"] = "I don't know what to say, would you like to teach me? (Say 'yes' or 'no')"
    # current_game_row["state"] = "interactive"
    # current_game_row["sub_state"] = "mayLearn"
    current_game_row["response"] = "I don't know what to say. What should I say when you say '" + sentence + "'? Say 'cancel' if you don't want to teach me a response"
    current_game_row["state"] = "learn"
    current_game_row["sub_state"] = "learn"
    current_game_row["sentence"] = sentence
    current_game_row["prev_sentence"] = last_game_row["sentence"]
    current_game_row["learn_sentence"] = current_game_row["sentence"]
    return current_game_row

def updateMaliciousData(last_game_row, current_game_row):
    # get the vector of the inappropriate response
    text = [last_game_row["problematic_resp"]]
    message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
    prob_res_vector = helper.get_features(message_embeddings)

    #update malicious data
    original_user = getOriginalUser(current_game_row['problematic_rowId'])
    id = helper.update_malicious_data(current_game_row["learn_sentence"], current_game_row['problematic_resp'],
                                      int(current_game_row["game_id"]), current_game_row['problematic_rowId'],
                                      prob_res_vector, 'safebot', original_user)
    maliciousData.append(
        [id, current_game_row["learn_sentence"], current_game_row['problematic_resp'], current_game_row["game_id"],
         datetime.now(),
         prob_res_vector, original_user])

# http://localhost:5000/api/getResponse?id=gameid&sentence=usersentence&investBot=True
@app.route('/api/getResponse', methods=['GET'])
def api_response():
    if 'sentence' in request.args:
        sentence = request.args['sentence']
    else:
        return "Error: No sentence field provided."

    is_safebot = request.args['investBot']
    # is_safebot = 'False' # for now- only chatbot, no investigation
    if is_safebot == 'True':
        chatType = "safebot"
    else:
        chatType = "safebot_no_invest"
    # chatType = "safebot_no_invest"  # for now- only chatbot, no investigation
    game_id = request.args['id']
    game_details = helper.getGameData(game_id, chatType)

    last_game_row = helper.init_rows()
    current_game_row = {}
    next_game_row = {}

    count_user = []
    temp_count_user = helper.get_user_count(chatType, game_id)
    if temp_count_user == None:
        count_user = [0,0]
    else:
        count_user = [temp_count_user[0], temp_count_user[1]]

    if len(game_details) == 0:
        # create new row in DB and local
        # helper.insertNewGame(game_id, 0, "interactive", "interactive", sentence, "safebot")
        # game_details.append(
        #     [int(game_id), 0, str(datetime.now()), None, "interactive", "interactive", None, None, None, None, sentence])
        # take the last row and save it as the current row
        # last_game_row = game_details[-1]
        current_game_row["game_id"] = game_id
        current_game_row["row"] = 0
        current_game_row["start_date"] = datetime.now()
        current_game_row["end_date"] = datetime.min
        current_game_row["state"] = "interactive"
        current_game_row["sub_state"] = "interactive"
        current_game_row["sentence"] = sentence
    else:
        # take the last row and save it as the current row
        # i am here - take the row and save it as dictionary
        max_row = max(game_details, key=lambda x: x[1])

        last_game_row["game_id"] = max_row[0]
        last_game_row["row"] = max_row[1]
        last_game_row["start_date"] = max_row[2]
        last_game_row["end_date"] = max_row[3]
        last_game_row["state"] = max_row[4]
        last_game_row["sub_state"] = max_row[5]
        last_game_row["prev_sentence"] = max_row[6]
        last_game_row["learn_sentence"] = max_row[7]
        last_game_row["problematic_resp"] = max_row[8]
        last_game_row["problematic_rowId"] = max_row[9]
        last_game_row["sentence"] = max_row[10]
        last_game_row["response"] = max_row[11]

        current_game_row["state"] = last_game_row["state"]
        current_game_row["sub_state"] = last_game_row["sub_state"]
        current_game_row["row"] = last_game_row["row"] + 1
        current_game_row["start_date"] = datetime.now()
        current_game_row["game_id"] = last_game_row["game_id"]
        current_game_row["prev_sentence"] = last_game_row["sentence"]
        current_game_row["sentence"] = sentence

    if current_game_row["state"] == "interactive":
        if current_game_row["sub_state"] == "interactive":
            cosineList = []
            if type(sentence) is str:
                text = [sentence]
            message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
            vector = helper.get_features(message_embeddings)
            vec_array = ast.literal_eval(vector)
            if is_safebot == 'True':
                for item in mainData:
                    distance = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(item[9]))
                    cosineList.append((item[0], item[2], item[7], distance, vec_array))
            else:
                for item in mainData_simp:
                    distance = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(item[9]))
                    cosineList.append((item[0], item[2], item[7], distance, vec_array))
            if len(cosineList) > 0:
                resp = min(cosineList, key=operator.itemgetter(3))

                if resp[2] == 1: # regular response
                    if resp[3] <= threshold: #The response is close enough to another response we have
                        # save the last response details and use it if we need
                        prev_details['rowid'] = resp[0]
                        prev_details['resp'] = resp[1]
                        prev_details['vector'] = resp[4]
                        # prev_details['sentence'] = sentence

                        current_game_row["problematic_resp"] = resp[1]
                        current_game_row["problematic_rowId"] = resp[0]
                        current_game_row["response"] = resp[1]
                        current_game_row["prev_sentence"] = last_game_row["sentence"]
                        current_game_row["learn_sentence"] = ''
                        output["response"] = current_game_row["response"]
                        output["state"] = current_game_row["state"]
                        output["sub_state"] = current_game_row["sub_state"]
                        helper.insertNewGame(current_game_row, chatType)
                        helper.countUseSentence(resp[0], chatType)
                        return jsonify(output)
                    else: # The response is not close enough to any response, we want to ask the user if he want to teach us a new response
                        current_game_row = dontKnowWhatToSayResp(current_game_row, last_game_row, sentence)
                        output["response"] = current_game_row["response"]
                        output["state"] = current_game_row["state"]
                        output["sub_state"] = current_game_row["sub_state"]
                        helper.insertNewGame(current_game_row, chatType)
                        return jsonify(output)
                else: # investigation?
                    current_count = helper.currentCountUse(last_game_row["problematic_rowId"], chatType)
                    if current_count < 10:
                        # save previous details in order to use it in investigation state
                        current_game_row["problematic_resp"] = last_game_row["problematic_resp"]
                        current_game_row["problematic_rowId"] = last_game_row["problematic_rowId"]
                        current_game_row["response"] = "I'm sorry, did I say something wrong? (Say 'yes' or 'no')"
                        output["response"] = current_game_row["response"]
                        current_game_row["state"] = "investigation"
                        current_game_row["sub_state"] = "investigation"
                        current_game_row["learn_sentence"] = current_game_row["prev_sentence"]
                        output["state"] = current_game_row["state"]
                        output["sub_state"] = current_game_row["sub_state"]
                        helper.insertNewGame(current_game_row, chatType)
                        helper.updateCurrentCountUse(current_game_row["problematic_rowId"], chatType, current_count)
                        return jsonify(output)
                    else:
                        current_game_row["response"] = "Sorry about that, let's keep chatting"
                        current_game_row["state"] = "interactive"
                        current_game_row["sub_state"] = "interactive"
                        current_game_row["sentence"] = sentence
                        current_game_row["problematic_rowId"] = last_game_row["problematic_rowId"]
                        current_game_row["problematic_resp"] = ""
                        current_game_row["learn_sentence"] = ""
                        output["response"] = current_game_row["response"]
                        output["state"] = current_game_row["state"]
                        output["sub_state"] = current_game_row["sub_state"]
                        helper.insertNewGame(current_game_row, chatType)
                        helper.updateCurrentCountUse(current_game_row["problematic_rowId"], chatType, current_count)
                        current_game_row["problematic_rowId"] = 0
                        return jsonify(output)


            else:
                # the table is empty
                #suggest him to teach
                current_game_row["problematic_resp"] = ""
                current_game_row["problematic_rowId"] = 0
                #current_game_row["response"] = "I don't know what to say, would you like to teach me? (Say 'yes' or 'no')"
                # current_game_row["state"] = "interactive"
                # current_game_row["sub_state"] = "mayLearn"
                current_game_row["response"] = "I don't know what to say. What should I say when you say " + sentence + "? Say 'cancel' if you don't want to teach me a response"
                current_game_row["state"] = "learn"
                current_game_row["sub_state"] = "learn"
                current_game_row["sentence"] = sentence
                current_game_row["prev_sentence"] = last_game_row["sentence"]
                current_game_row["learn_sentence"] = current_game_row["sentence"]
                output["response"] = current_game_row["response"]
                output["state"] = current_game_row["state"]
                output["sub_state"] = current_game_row["sub_state"]
                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)
        if current_game_row["sub_state"] == 'mayLearn': # we already asked the use if he want want to teach us a new response
            if type(sentence) is str:
                text = [sentence]
            message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
            vector = helper.get_features(message_embeddings)
            vec_array = ast.literal_eval(vector)
            distanceYes = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_yes))
            distanceNo = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_no))
            if distanceYes < distanceNo: #the user said something closer to 'yes' then to 'no'
                current_game_row["response"] = "What should I say when you say '" + current_game_row["prev_sentence"] + \
                                               "'? (Say 'incorrect' if this is not what you want to teach)"
                current_game_row["state"] = "learn"
                current_game_row["sub_state"] = "directLearning"
                current_game_row["sentence"] = sentence
                current_game_row["problematic_rowId"] = 0
                current_game_row["problematic_resp"] = ""
                current_game_row["learn_sentence"] = last_game_row["learn_sentence"]
                output["response"] = current_game_row["response"]
                output["state"] = current_game_row["state"]
                output["sub_state"] = current_game_row["sub_state"]
                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)
            else: # the user said something closer to 'no'
                current_game_row["response"] = "OK, I won't learn anything, let's keep chatting"
                current_game_row["state"] = "interactive"
                current_game_row["sub_state"] = "interactive"
                current_game_row["sentence"] = sentence
                current_game_row["problematic_rowId"] = 0
                current_game_row["problematic_resp"] = ""
                current_game_row["learn_sentence"] = ""
                output["response"] = current_game_row["response"]
                output["state"] = current_game_row["state"]
                output["sub_state"] = current_game_row["sub_state"]
                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)
    if current_game_row['state'] == "investigation":
        if current_game_row['sub_state'] == "investigation":
            if type(sentence) is str:
                text = [sentence]
            message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
            vector = helper.get_features(message_embeddings)
            vec_array = ast.literal_eval(vector)
            distanceYes = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_yes))
            distanceNo = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_no))
            if distanceYes < distanceNo: #the user said something close to 'yes'
                current_game_row["response"] = "Was my response very inappropriate, or just not related to what you have just said?"
                output["response"] = current_game_row["response"]
                current_game_row["state"] = "investigation"
                current_game_row["sub_state"] = "type"
                current_game_row["problematic_resp"] = last_game_row["problematic_resp"]
                current_game_row["problematic_rowId"] = last_game_row["problematic_rowId"]
                current_game_row["learn_sentence"] = last_game_row["learn_sentence"]
                output["state"] = current_game_row["state"]
                output["sub_state"] = current_game_row["sub_state"]

                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)
            else: #the user said something close to 'no'
                current_game_row["response"] = "I didn't understand you, let's keep chatting"
                output["response"] = current_game_row["response"]
                current_game_row["state"] = "interactive"
                current_game_row["sub_state"] = "interactive"
                current_game_row["sentence"] = sentence
                current_game_row["problematic_rowId"] = 0
                current_game_row["problematic_resp"] = ""
                current_game_row["learn_sentence"] = ""
                output["state"] = current_game_row['state']
                output["sub_state"] = current_game_row['sub_state']
                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)
        if current_game_row['sub_state'] == "type":
            vec_inapprop = []
            vec_not_related = []
            for item in criticismData:
                if item[1] == 'inappropriate':
                    vec_inapprop = item[3]
                if item[1] == 'not related':
                    vec_not_related = item[3]
            if type(sentence) is str:
                text = [sentence]
            message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
            vector = helper.get_features(message_embeddings)
            vec_array = ast.literal_eval(vector)
            distanceInapprop = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_inapprop))
            distanceNotRelated = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_not_related))

            current_game_row["problematic_resp"] = last_game_row["problematic_resp"]
            current_game_row["problematic_rowId"] = last_game_row["problematic_rowId"]
            current_game_row["learn_sentence"] = last_game_row["learn_sentence"]


            if distanceInapprop < distanceNotRelated: # means, the response is probably inappropriate
                if count_user == None or count_user[0] == 0:
                    updateMaliciousData(last_game_row, current_game_row)
                    count_user[0] = 1
                    helper.updateCountUser(chatType, game_id, count_user)

                originalUser = getOriginalUser(current_game_row["problematic_rowId"])
                if helper.malicious_resps_num(chatType,originalUser) > 1:
                    delRelatedRows(originalUser)

            # delete from main data (this is relevant only for the case of safebot)
            deleteMainData(current_game_row['problematic_rowId'])

            current_game_row["response"] = "I won't say it any more. What should I say instead when you say '" + current_game_row['learn_sentence'] + "'? say 'cancel' if you don't want to teach a new response"
            output["response"] = current_game_row["response"]
            # current_game_row["state"] = "investigation"
            # current_game_row["sub_state"] = "mayLearn"
            current_game_row["state"] = "learn"
            current_game_row["sub_state"] = "learn"
            output["state"] = current_game_row["state"]
            output["sub_state"] = current_game_row["sub_state"]
            helper.insertNewGame(current_game_row, chatType)
            return jsonify(output)

        if current_game_row['sub_state'] == "mayLearn":
            if type(sentence) is str:
                text = [sentence]
            message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
            vector = helper.get_features(message_embeddings)
            vec_array = ast.literal_eval(vector)
            distanceYes = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_yes))
            distanceNo = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_no))
            if distanceYes < distanceNo: #the user said something close to 'yes'
                current_game_row["learn_sentence"] = last_game_row["learn_sentence"]
                current_game_row["problematic_rowId"] = last_game_row["problematic_rowId"]
                current_game_row["problematic_resp"] = last_game_row["problematic_resp"]
                current_game_row["response"] = "What should I say when you say '" + current_game_row["learn_sentence"] + "'?"
                output["response"] = current_game_row["response"]
                current_game_row["state"] = "learn"
                current_game_row["sub_state"] = "fixResponse"
                output["state"] = current_game_row["state"]
                output["sub_state"] = current_game_row["sub_state"]
                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)
            else: #the user said something close to 'no'
                current_game_row["response"] = "OK, let's keep chatting"
                output["response"] = current_game_row["response"]
                current_game_row["state"] = "interactive"
                current_game_row["sub_state"] = "interactive"
                output["state"] = current_game_row["state"]
                current_game_row['learn_sentence'] = ''
                current_game_row["problematic_resp"] = ''
                current_game_row["problematic_rowId"] = 0
                output["sub_state"] = current_game_row['sub_state']
                helper.insertNewGame(current_game_row, chatType)
                return jsonify(output)

    if current_game_row["state"] == "learn":
        sentence = sentence.strip()
        keys = '|'.join(['ask', 'asks', 'asked', 'say', 'says', 'said'])
        sen = re.search(rf'\b({keys})\b\W+(.*)', sentence, re.S | re.I)

        #sen = re.split(r'(?:\bsa(?:ys?|id)|ask(?:ed|s))?\b[,.;:?!]*', sentence, 1, re.I)[-1]
        # if sen != '':
        #     sen = sen.strip()
        #     current_game_row["sentence"] = sen

        if sen != None:
            sentence = sen.group(2)
            current_game_row["sentence"] = sentence



        if type(current_game_row["sentence"]) is str:
            text = [current_game_row["sentence"]]
        message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
        vector = helper.get_features(message_embeddings)
        vec_array = ast.literal_eval(vector)

        #check if he said 'incorrent'
        # distanceIncorrect = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_incorrect))
        # if (distanceIncorrect >= incorrect_thershold):
        disCancel = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(vec_cancel))
        if (disCancel >= incorrect_thershold):
            if count_user[1] < 3:
                #check the response against responses in malicious data
                if chatType == "safebot":
                    a = maliciousData
                else:
                    a = maliciousData_simp
                for item in a:
                    distance = scipy.spatial.distance.cosine(vec_array, ast.literal_eval(item[5]))
                    if distance < malicious_threshold:
                        current_game_row["response"] = """The response you have just tried to teach is suspected as inappropriate and won't be learned. \
                                                         let's keep chatting"""
                        count_user[1] += 1
                        helper.updateCountUser(chatType, game_id, count_user)
                        break
                else:
                    if type(last_game_row["learn_sentence"]) is str:
                        text = [last_game_row["learn_sentence"]]
                    message_embeddings = session_use.run(embedded_text, feed_dict={text_input: text})
                    vector_sent = helper.get_features(message_embeddings)
                    id = helper.insert_main_data(last_game_row["learn_sentence"], current_game_row["sentence"], current_game_row["game_id"], 2,  vector_sent, chatType)
                    if is_safebot == 'True':
                        mainData.append(
                            [id, last_game_row["learn_sentence"], current_game_row["sentence"], current_game_row["game_id"], 2, datetime.now(),
                            None, 1,1 ,vector_sent])
                    else:
                        mainData_simp.append(
                            [id, last_game_row["learn_sentence"], current_game_row["sentence"], current_game_row["game_id"],
                             2, datetime.now(),
                             None, 1, 1, vector_sent])
                    current_game_row["response"] = """OK, I have learned the new response.
                                                    let's keep chatting"""
            else:
                current_game_row["response"] = "Sorry, I can't learn that response"
            # anyway, we return the same data
            current_game_row["state"] = "interactive"
            current_game_row["sub_state"] = "interactive"
            current_game_row["sentence"] = sentence
            current_game_row["problematic_rowId"] = 0
            current_game_row["problematic_resp"] = ""
            current_game_row["learn_sentence"] = ""
            output["response"] = current_game_row["response"]
            output["state"] = current_game_row["state"]
            output["sub_state"] = current_game_row["sub_state"]
            helper.insertNewGame(current_game_row, chatType)
            return jsonify(output)
        else:
            # The user responses something close to 'incorrect'
            current_game_row["response"] = """OK, I won't learn anything.
                                              let's keep chatting"""
            current_game_row["state"] = "interactive"
            current_game_row["sub_state"] = "interactive"
            current_game_row["sentence"] = sentence
            current_game_row["problematic_rowId"] = 0
            current_game_row["problematic_resp"] = ""
            current_game_row["learn_sentence"] = ""
            output["response"] = current_game_row["response"]
            output["state"] = current_game_row["state"]
            output["sub_state"] = current_game_row["sub_state"]
            helper.insertNewGame(current_game_row, chatType)
            return jsonify(output)
    return jsonify("hello")


app.run()